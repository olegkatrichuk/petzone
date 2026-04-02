#!/usr/bin/env python3
"""
Seed adoption listings with real data + real photos into PetZone.

Data:   Austin Animal Center (data.austintexas.gov) — real shelter animals
Photos: dog.ceo (dogs), cataas.com (cats), flickr public domain (others)
All sources are free and require no API keys.

Usage:
    pip install requests
    python scripts/seed_listings.py
"""

import sys
import random
import uuid
import io
import requests

# ── Local API ─────────────────────────────────────────────────────────────────
API_BASE       = "http://localhost:5183"
ADMIN_EMAIL    = "admin@petzone.com"
ADMIN_PASSWORD = "Admin1234!"

# ── Data source ────────────────────────────────────────────────────────────────
AAC_URL = (
    "https://data.austintexas.gov/resource/9t4d-g238.json"
    "?$limit=150&$order=datetime DESC"
)

LISTINGS_TO_SEED = 15
PHOTOS_PER_LISTING = 2   # 1-2 photos each

# ── Photo sources (no API key needed) ─────────────────────────────────────────
DOG_PHOTO_API = "https://dog.ceo/api/breeds/image/random"
CAT_PHOTO_API = "https://cataas.com/cat?json=true"
RABBIT_PHOTO_BREEDS = ["rex", "lionhead", "dutch", "angora"]

UA_CITIES = [
    "Київ", "Харків", "Одеса", "Дніпро", "Запоріжжя",
    "Львів", "Миколаїв", "Херсон", "Чернігів", "Полтава",
    "Суми", "Вінниця", "Луцьк", "Тернопіль", "Кривий Ріг",
]

COLOR_MAP = {
    "black": "Чорний", "white": "Білий", "brown": "Коричневий",
    "brown/white": "Коричнево-білий", "black/white": "Чорно-білий",
    "orange": "Рудий", "orange/white": "Рудо-білий",
    "gray": "Сірий", "gray/white": "Сіро-білий",
    "tan": "Бежевий", "tan/white": "Бежево-білий",
    "yellow": "Жовтий", "cream": "Кремовий",
    "tricolor": "Трьохколірний", "blue": "Блакитний",
    "calico": "Триколірний", "tortie": "Черепаховий",
    "red": "Рудий", "apricot": "Абрикосовий",
}

SPECIES_UA = {"dog": "пес", "cat": "кіт", "rabbit": "кролик", "bird": "птах"}

TITLE_TEMPLATES = [
    "Віддам {sp} в добрі руки",
    "Шукаємо люблячу родину для {sp}а",
    "Безкоштовно віддам {sp}а",
    "Чудовий {sp} шукає господаря",
    "Відповідальних господарів для {sp}а",
    "{sp} шукає дім з турботою",
    "Терміново! Потрібен дім для {sp}а",
]

DESC_EXTRAS = [
    "Дуже ласкавий та дружелюбний, любить людей.",
    "Добре уживається з дітьми та іншими тваринами.",
    "Привчений до лотку / нашийника.",
    "Полюбляє прогулянки та активні ігри.",
    "Спокійний характер, підійде для квартири.",
    "Шукаємо відповідальну родину, яка подарує йому тепло та турботу.",
    "Дуже кмітливий, легко навчається командам.",
    "Буде вдячний кожній ласці та увазі.",
]

AGE_MONTHS = {"Baby": 3, "Young": 12, "Adult": 36, "Senior": 96}


# ── Auth ───────────────────────────────────────────────────────────────────────

def get_local_token() -> str:
    print("🔐 Логінимось...")
    r = requests.post(f"{API_BASE}/accounts/login",
                      json={"email": ADMIN_EMAIL, "password": ADMIN_PASSWORD}, timeout=10)
    r.raise_for_status()
    d = r.json()
    token = (d.get("accessToken")
             or (d.get("result") or {}).get("accessToken")
             or (d.get("value") or {}).get("accessToken"))
    if not token:
        print("❌ Токен не знайдено:", d); sys.exit(1)
    print("✅ Токен отримано.")
    return token


# ── Species ────────────────────────────────────────────────────────────────────

def get_local_species(token: str) -> list[dict]:
    h = {"Authorization": f"Bearer {token}"}
    r = requests.get(f"{API_BASE}/species", headers=h, timeout=10)
    r.raise_for_status()
    raw = r.json()
    if isinstance(raw, dict):
        raw = raw.get("result") or raw.get("value") or raw.get("data") or []
    result = []
    for s in raw:
        sid = s["id"]
        name_en = (s.get("translations") or {}).get("en", "").lower()
        breeds = []
        br = requests.get(f"{API_BASE}/species/{sid}/breeds", headers=h, timeout=10)
        if br.ok:
            br_raw = br.json()
            if isinstance(br_raw, dict):
                br_raw = br_raw.get("result") or br_raw.get("value") or br_raw.get("data") or []
            for b in (br_raw or []):
                breeds.append({"id": b["id"], "name_en": (b.get("translations") or {}).get("en", "").lower()})
        result.append({"id": sid, "name_en": name_en, "breeds": breeds})
    print(f"📋 Видів: {len(result)} — {[s['name_en'] for s in result]}")
    return result


def find_species(animal_type: str, local_species: list[dict]) -> dict | None:
    at = animal_type.lower()
    for s in local_species:
        if at in s["name_en"] or s["name_en"] in at:
            return s
    return None


# ── Photo fetching ─────────────────────────────────────────────────────────────

def fetch_dog_photo_bytes() -> tuple[bytes, str] | None:
    """Returns (image_bytes, extension)."""
    try:
        r = requests.get(DOG_PHOTO_API, timeout=8)
        if not r.ok:
            return None
        url = r.json().get("message", "")
        if not url:
            return None
        img = requests.get(url, timeout=15)
        if not img.ok:
            return None
        ext = url.split(".")[-1].split("?")[0].lower() or "jpg"
        return img.content, ext
    except Exception:
        return None


def fetch_cat_photo_bytes() -> tuple[bytes, str] | None:
    try:
        # cataas returns JSON with URL
        r = requests.get(CAT_PHOTO_API, timeout=8)
        if r.ok:
            data = r.json()
            url = data.get("url") or data.get("_id")
            if url:
                if not url.startswith("http"):
                    url = f"https://cataas.com{url}"
                img = requests.get(url, timeout=15)
                if img.ok:
                    return img.content, "jpg"
        # fallback — direct image
        img = requests.get("https://cataas.com/cat", timeout=15)
        if img.ok:
            return img.content, "jpg"
    except Exception:
        pass
    return None


def fetch_rabbit_photo_bytes() -> tuple[bytes, str] | None:
    breed = random.choice(RABBIT_PHOTO_BREEDS)
    try:
        r = requests.get(f"https://dog.ceo/api/breed/{breed}/images/random", timeout=8)
        # dog.ceo has no rabbits, so fall back to a neutral animal image
    except Exception:
        pass
    # Use a public domain rabbit photo via Wikimedia
    urls = [
        "https://upload.wikimedia.org/wikipedia/commons/thumb/1/1f/Oryctolagus_cuniculus_Rcdo.jpg/320px-Oryctolagus_cuniculus_Rcdo.jpg",
        "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5a/Rabbit_in_montana.jpg/320px-Rabbit_in_montana.jpg",
    ]
    try:
        img = requests.get(random.choice(urls), timeout=15)
        if img.ok:
            return img.content, "jpg"
    except Exception:
        pass
    return None


def get_photo_bytes(species_en: str) -> tuple[bytes, str] | None:
    if "dog" in species_en:
        return fetch_dog_photo_bytes()
    elif "cat" in species_en:
        return fetch_cat_photo_bytes()
    elif "rabbit" in species_en:
        return fetch_rabbit_photo_bytes()
    else:
        return fetch_dog_photo_bytes()


# ── File upload ────────────────────────────────────────────────────────────────

def upload_photo(image_bytes: bytes, ext: str, token: str) -> str | None:
    """Upload photo to MinIO via /files/upload. Returns stored fileName."""
    fname = f"{uuid.uuid4()}.{ext}"
    try:
        r = requests.post(
            f"{API_BASE}/files/upload",
            headers={"Authorization": f"Bearer {token}"},
            files={"file": (fname, io.BytesIO(image_bytes), f"image/{ext}")},
            timeout=20,
        )
        if not r.ok:
            print(f"    ⚠️  Upload failed {r.status_code}: {r.text[:100]}")
            return None
        data = r.json()
        stored = (data.get("result") or data.get("value") or data.get("fileName") or fname)
        if isinstance(stored, dict):
            stored = stored.get("fileName") or fname
        return stored
    except Exception as e:
        print(f"    ⚠️  Upload error: {e}")
        return None


def attach_photo_to_listing(listing_id: str, file_name: str, token: str) -> bool:
    r = requests.post(
        f"{API_BASE}/listings/{listing_id}/photos",
        json={"fileName": file_name},
        headers={"Authorization": f"Bearer {token}"},
        timeout=10,
    )
    return r.ok


# ── Data transform ─────────────────────────────────────────────────────────────

def map_color(raw: str) -> str:
    if not raw:
        return "Різний"
    key = raw.lower().strip()
    if key in COLOR_MAP:
        return COLOR_MAP[key]
    for k, v in COLOR_MAP.items():
        if k in key:
            return v
    return raw.title()


def age_str_to_months(age_str: str) -> int:
    if not age_str:
        return 24
    age_str = age_str.lower()
    years = months = 0
    parts = age_str.split()
    for i, p in enumerate(parts):
        if p.isdigit():
            val = int(p)
            if i + 1 < len(parts):
                unit = parts[i + 1]
                if "year" in unit:
                    years = val
                elif "month" in unit:
                    months = val
                elif "week" in unit:
                    months = max(1, val // 4)
    total = years * 12 + months
    return total if total > 0 else 12


def build_description(animal: dict, sp_ua: str) -> str:
    breed  = animal.get("breed", "").strip()
    sex_en = (animal.get("sex") or "").lower()
    color  = map_color(animal.get("color", ""))
    sex_ua = "хлопчик" if sex_en == "male" else "дівчинка" if sex_en == "female" else ""
    lines  = []
    if sex_ua:
        lines.append(f"Стать: {sex_ua}.")
    if breed and breed.lower() not in ("unknown", ""):
        lines.append(f"Порода: {breed}.")
    if color and color != "Різний":
        lines.append(f"Колір: {color}.")
    lines += random.sample(DESC_EXTRAS, 2)
    lines.append("Будь ласка, зв'яжіться з нами для знайомства з тваринкою.")
    return " ".join(lines)


def transform(animal: dict, local_species: list[dict]) -> dict | None:
    sp = find_species(animal.get("animal_type", ""), local_species)
    if sp is None:
        return None
    sp_key = animal.get("animal_type", "").lower()
    sp_ua  = SPECIES_UA.get(sp_key, sp_key)
    return {
        "title":       random.choice(TITLE_TEMPLATES).format(sp=sp_ua).capitalize(),
        "description": build_description(animal, sp_ua),
        "speciesId":   sp["id"],
        "breedId":     None,
        "ageMonths":   max(1, age_str_to_months(animal.get("age_upon_outcome") or animal.get("age_upon_intake") or "")),
        "color":       map_color(animal.get("color", "")),
        "city":        random.choice(UA_CITIES),
        "vaccinated":  random.choice([True, False]),
        "castrated":   random.choice([True, False]),
        "phone":       None,
        "_species_en": sp_key,
    }


# ── Post listing ───────────────────────────────────────────────────────────────

def post_listing(payload: dict, token: str) -> str | None:
    body = {k: v for k, v in payload.items() if not k.startswith("_")}
    r = requests.post(f"{API_BASE}/listings", json=body,
                      headers={"Authorization": f"Bearer {token}"}, timeout=10)
    if not r.ok:
        print(f"    ⚠️  {r.status_code}: {r.text[:150]}")
        return None
    data = r.json()
    lid = (data.get("result") or data.get("value") or data.get("id") or {})
    if isinstance(lid, dict):
        lid = lid.get("id")
    return str(lid) if lid else None


# ── Main ───────────────────────────────────────────────────────────────────────

def main():
    token         = get_local_token()
    local_species = get_local_species(token)

    if not local_species:
        print("❌ У базі немає species.")
        sys.exit(1)

    print(f"🐾 Завантажуємо дані з Austin Animal Center...")
    r = requests.get(AAC_URL, timeout=15)
    r.raise_for_status()
    raw_animals = r.json()
    print(f"   Отримано: {len(raw_animals)} записів")

    payloads: list[dict] = []
    for a in raw_animals:
        p = transform(a, local_species)
        if p:
            payloads.append(p)
        if len(payloads) >= LISTINGS_TO_SEED:
            break

    if not payloads:
        print("❌ Не вдалось сформувати оголошення.")
        sys.exit(1)

    print(f"\n📤 Публікуємо {len(payloads)} оголошень з фото...\n")
    ok = 0
    for i, payload in enumerate(payloads, 1):
        species_en = payload.get("_species_en", "dog")

        # 1 — create listing
        listing_id = post_listing(payload, token)
        if not listing_id:
            print(f"  ❌ [{i}/{len(payloads)}] {payload['title']}")
            continue

        # 2 — upload & attach photos
        photos_ok = 0
        for _ in range(PHOTOS_PER_LISTING):
            photo = get_photo_bytes(species_en)
            if not photo:
                continue
            img_bytes, ext = photo
            file_name = upload_photo(img_bytes, ext, token)
            if file_name and attach_photo_to_listing(listing_id, file_name, token):
                photos_ok += 1

        photo_icon = f"📷×{photos_ok}" if photos_ok else "🚫фото"
        print(f"  ✅ [{i}/{len(payloads)}] {payload['title']} | {payload['city']} | {photo_icon}")
        ok += 1

    print(f"\n🎉 Готово! Опубліковано: {ok}/{len(payloads)} оголошень з реальними фото.")


if __name__ == "__main__":
    main()
