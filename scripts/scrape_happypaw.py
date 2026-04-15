#!/usr/bin/env python3
"""Scrape happypaw.ua/ua/zoo-organization and save to frontend/src/data/shelters.json"""

import json
import re
import time
import requests
import warnings
from html.parser import HTMLParser
warnings.filterwarnings('ignore', message='Unverified HTTPS request')

BASE = "https://happypaw.ua"
LIST_URL = BASE + "/ua/zoo-organization?page={page}"
DETAIL_RE = re.compile(r'/ua/zoo-organization/(\d+)$')
SESSION = requests.Session()
SESSION.headers.update({
    'User-Agent': 'Mozilla/5.0 (compatible; PetZone-scraper/1.0)',
    'Accept-Language': 'uk,en;q=0.9',
})

# ── HTML Parsers ────────────────────────────────────────────────────────────────

class ListParser(HTMLParser):
    """Extract org cards from the listing page."""
    def __init__(self):
        super().__init__()
        self.orgs = []
        self._in_card = False
        self._href = None
        self._name = None
        self._city = None
        self._grab_name = False
        self._grab_city = False
        self._img = None

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == 'a' and 'href' in attrs and DETAIL_RE.match(attrs['href']):
            self._in_card = True
            self._href = BASE + attrs['href']
            self._name = None
            self._city = None
            self._img = None
        if self._in_card:
            if tag == 'h4':
                self._grab_name = True
            if tag == 'p':
                self._grab_city = True
            if tag == 'img' and 'src' in attrs:
                self._img = attrs['src']
                if not self._img.startswith('http'):
                    self._img = BASE + self._img

    def handle_endtag(self, tag):
        if tag == 'a' and self._in_card:
            if self._name:
                self.orgs.append({
                    'url': self._href,
                    'name': self._name.strip(),
                    'city': (self._city or '').strip(),
                    'photo': self._img,
                })
            self._in_card = False
            self._grab_name = False
            self._grab_city = False
        if tag == 'h4':
            self._grab_name = False
        if tag == 'p':
            self._grab_city = False

    def handle_data(self, data):
        if self._grab_name:
            self._name = (self._name or '') + data
        elif self._grab_city:
            self._city = (self._city or '') + data


class DetailParser(HTMLParser):
    """Extract phone, email, facebook, instagram, description from detail page."""
    def __init__(self):
        super().__init__()
        self.phone = None
        self.email = None
        self.facebook = None
        self.instagram = None
        self.website = None
        self.description = None
        self._in_desc = False
        self._desc_depth = 0
        self._desc_buf = []

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        href = attrs.get('href', '')
        if tag == 'a':
            if href.startswith('tel:') and not self.phone:
                self.phone = href[4:].strip()
            elif href.startswith('mailto:') and not self.email:
                self.email = href[7:].strip()
            elif 'facebook.com' in href and not self.facebook:
                self.facebook = href
            elif 'instagram.com' in href and not self.instagram:
                self.instagram = href
            elif href.startswith('http') and 'happypaw' not in href and not self.website:
                if not any(x in href for x in ['facebook', 'instagram', 'twitter', 'youtube', 'viber', 'telegram']):
                    self.website = href
        # grab description from field--name-body or field--name-field-description
        cls = attrs.get('class', '')
        if 'field--name-body' in cls or 'field--name-field-description' in cls:
            self._in_desc = True
            self._desc_depth = 0

    def handle_endtag(self, tag):
        if self._in_desc:
            if tag in ('div', 'article'):
                self._desc_depth -= 1
                if self._desc_depth < 0:
                    self.description = ' '.join(self._desc_buf).strip()[:500]
                    self._in_desc = False

    def handle_data(self, data):
        if self._in_desc:
            t = data.strip()
            if t:
                self._desc_buf.append(t)


def fetch(url, retries=3):
    for attempt in range(retries):
        try:
            r = SESSION.get(url, timeout=15, verify=False)
            r.raise_for_status()
            return r.text
        except Exception as e:
            if attempt == retries - 1:
                print(f"  FAILED {url}: {e}")
                return ''
            time.sleep(2)


def scrape_list_pages():
    all_orgs = []
    for page in range(24):
        url = LIST_URL.format(page=page)
        print(f"List page {page + 1}/24 …")
        html = fetch(url)
        p = ListParser()
        p.feed(html)
        all_orgs.extend(p.orgs)
        time.sleep(0.5)
    return all_orgs


def scrape_details(orgs):
    total = len(orgs)
    for i, org in enumerate(orgs):
        print(f"Detail {i + 1}/{total}: {org['name'][:50]}")
        html = fetch(org['url'])
        if not html:
            continue
        p = DetailParser()
        p.feed(html)
        org['phone'] = p.phone
        org['email'] = p.email
        org['facebook'] = p.facebook
        org['instagram'] = p.instagram
        org['website'] = p.website
        org['description'] = p.description
        time.sleep(0.4)
    return orgs


def main():
    import os
    print("Step 1: Scraping list pages …")
    orgs = scrape_list_pages()
    print(f"  Found {len(orgs)} organizations")

    print("\nStep 2: Scraping detail pages …")
    orgs = scrape_details(orgs)

    out_path = os.path.join(os.path.dirname(__file__), '..', 'frontend', 'src', 'data', 'shelters.json')
    out_path = os.path.normpath(out_path)
    os.makedirs(os.path.dirname(out_path), exist_ok=True)

    with open(out_path, 'w', encoding='utf-8') as f:
        json.dump(orgs, f, ensure_ascii=False, indent=2)

    print(f"\nSaved {len(orgs)} orgs → {out_path}")


if __name__ == '__main__':
    main()
