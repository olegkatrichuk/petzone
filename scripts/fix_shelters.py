#!/usr/bin/env python3
"""Fix scraped shelters.json: decode phones, remove default HappyPaw contacts, add cities."""

import json
import re
import time
import requests
import warnings
from html.parser import HTMLParser
from urllib.parse import unquote

warnings.filterwarnings('ignore', message='Unverified HTTPS request')

BASE = "https://happypaw.ua"
LIST_URL = BASE + "/ua/zoo-organization?page={page}"
DETAIL_RE = re.compile(r'/ua/zoo-organization/(\d+)$')

SESSION = requests.Session()
SESSION.headers.update({
    'User-Agent': 'Mozilla/5.0 (compatible; PetZone-scraper/1.0)',
    'Accept-Language': 'uk,en;q=0.9',
})

# HappyPaw's own default contacts to filter out
HAPPYPAW_DEFAULTS = {
    'emails': {'hello@happypaw.ua'},
    'facebooks': {'https://www.facebook.com/HappyPawFund/', 'https://www.facebook.com/HappyPawFund/?fref=ts'},
    'instagrams': {'https://www.instagram.com/happy_paw', 'https://www.instagram.com/happy_paw/'},
    'websites': {'https://www.linkedin.com/company/charity-fundation-happy-paw'},
}


class ListParser(HTMLParser):
    """Extract org cards: url → city mapping."""
    def __init__(self):
        super().__init__()
        self.url_to_city = {}
        self._in_card = False
        self._href = None
        self._city = None
        self._grab_city = False
        self._skip_h4 = False  # skip name inside h4

    def handle_starttag(self, tag, attrs):
        attrs_d = dict(attrs)
        href = attrs_d.get('href', '')
        if tag == 'a' and DETAIL_RE.match(href):
            self._in_card = True
            self._href = BASE + href
            self._city = None
            self._grab_city = False
            self._skip_h4 = False
        if self._in_card:
            if tag == 'h4':
                self._skip_h4 = True
            if tag == 'p' and not self._skip_h4 and self._city is None:
                self._grab_city = True

    def handle_endtag(self, tag):
        if tag == 'a' and self._in_card:
            if self._href and self._city:
                self.url_to_city[self._href] = self._city.strip()
            self._in_card = False
            self._grab_city = False
            self._skip_h4 = False
        if tag == 'h4':
            self._skip_h4 = False
        if tag == 'p':
            self._grab_city = False

    def handle_data(self, data):
        if self._grab_city:
            self._city = (self._city or '') + data


def fetch(url):
    try:
        r = SESSION.get(url, timeout=15, verify=False)
        r.raise_for_status()
        return r.text
    except Exception as e:
        print(f"  FAILED {url}: {e}")
        return ''


def scrape_cities():
    url_to_city = {}
    for page in range(24):
        print(f"List page {page + 1}/24 …")
        html = fetch(LIST_URL.format(page=page))
        p = ListParser()
        p.feed(html)
        url_to_city.update(p.url_to_city)
        time.sleep(0.4)
    return url_to_city


def clean_phone(phone):
    if not phone:
        return None
    decoded = unquote(phone)
    # keep only +, digits, spaces, dashes
    cleaned = re.sub(r'[^\d\+\-\s]', '', decoded).strip()
    return cleaned if cleaned else None


def clean_record(org, url_to_city):
    result = dict(org)

    # Fix city
    result['city'] = url_to_city.get(org['url']) or org.get('city') or ''
    # Extract just city (before first comma)
    city_full = result['city']
    city_parts = city_full.split(',')
    result['city'] = city_parts[0].strip() if city_parts else city_full

    # Fix phone
    result['phone'] = clean_phone(org.get('phone'))

    # Clear HappyPaw default email
    email = org.get('email', '')
    result['email'] = None if email in HAPPYPAW_DEFAULTS['emails'] else email

    # Clear HappyPaw default facebook
    fb = (org.get('facebook') or '').rstrip('/')
    result['facebook'] = None if (fb + '/') in HAPPYPAW_DEFAULTS['facebooks'] or fb in HAPPYPAW_DEFAULTS['facebooks'] else org.get('facebook')

    # Clear HappyPaw default instagram
    ig = (org.get('instagram') or '').rstrip('/')
    result['instagram'] = None if (ig + '/') in HAPPYPAW_DEFAULTS['instagrams'] or ig in HAPPYPAW_DEFAULTS['instagrams'] else org.get('instagram')

    # Clear HappyPaw default website
    ws = org.get('website') or ''
    result['website'] = None if ws in HAPPYPAW_DEFAULTS['websites'] else ws or None

    return result


def main():
    import os
    data_path = os.path.join(os.path.dirname(__file__), '..', 'frontend', 'src', 'data', 'shelters.json')
    data_path = os.path.normpath(data_path)

    with open(data_path, encoding='utf-8') as f:
        orgs = json.load(f)

    print(f"Loaded {len(orgs)} orgs")
    print("Scraping cities from list pages …")
    url_to_city = scrape_cities()
    print(f"Got cities for {len(url_to_city)} orgs")

    cleaned = [clean_record(org, url_to_city) for org in orgs]

    with open(data_path, 'w', encoding='utf-8') as f:
        json.dump(cleaned, f, ensure_ascii=False, indent=2)

    # Print stats
    has_phone = sum(1 for o in cleaned if o.get('phone'))
    has_email = sum(1 for o in cleaned if o.get('email'))
    has_fb = sum(1 for o in cleaned if o.get('facebook'))
    has_city = sum(1 for o in cleaned if o.get('city'))
    print(f"\nStats: phone={has_phone}, email={has_email}, facebook={has_fb}, city={has_city}")
    print(f"Saved → {data_path}")

    # Sample output
    print("\nSample (first 2):")
    for o in cleaned[:2]:
        print(json.dumps(o, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()