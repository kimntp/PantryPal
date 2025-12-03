import json
import os
import time
import re
import requests
from typing import Optional

# === CONFIG ===
RECIPES_JSON_PATH = "/Users/gh3work/code_projects/PantryPal/PPApp/Resources/Raw/finalizedDataset.json"
OUTPUT_DIR = "/Users/gh3work/code_projects/PantryPal/PPApp/Resources/Images/recipeImages"
REQUEST_DELAY_SECONDS = 1.0

def get_main_image_url_from_html(html_text: str) -> Optional[str]:
    """
    Try to extract a main image URL from the HTML using regex only:
    1) <meta property="og:image" content="...">
    2) Fallback: first <img src="...">
    """

    # 1) Look for og:image
    og_match = re.search(
        r'<meta[^>]+property=["\']og:image["\'][^>]+content=["\']([^"\']+)["\']',
        html_text,
        flags=re.IGNORECASE,
    )
    if og_match:
        return og_match.group(1)

    # 2) Fallback: first <img src="...">
    img_match = re.search(
        r'<img[^>]+src=["\']([^"\']+)["\']',
        html_text,
        flags=re.IGNORECASE,
    )
    if img_match:
        return img_match.group(1)

    return None

def download_image(url: str, dest_path: str):
    resp = requests.get(url, timeout=15)
    resp.raise_for_status()
    with open(dest_path, "wb") as f:
        f.write(resp.content)


def main():
    # Ensure output dir exists
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    # Load recipes
    with open(RECIPES_JSON_PATH, "r", encoding="utf-8") as f:
        recipes = json.load(f)

    print(f"Loaded {len(recipes)} recipes")

    for idx, recipe in enumerate(recipes):
        url = recipe.get("url")
        if not url:
            print(f"[{idx}] Skipping recipe with no URL")
            continue

        # Use numeric recipeID directly
        try:
            file_id = recipe["recipeID"]
        except KeyError:
            print(f"[{idx}] No recipeID field, skipping")
            continue

        out_path = os.path.join(OUTPUT_DIR, f"recipe_{file_id}.jpg")

        # Skip if we already have this image
        #if os.path.exists(out_path):
         #   print(f"[{file_id}] Image already exists, skipping")
          #  continue

        print(f"[{file_id}] Fetching page: {url}")

        try:
            page_resp = requests.get(url, timeout=15)
            page_resp.raise_for_status()
        except Exception as e:
            print(f"  ERROR fetching page: {e}")
            continue

        img_url = get_main_image_url_from_html(page_resp.text)
        if not img_url:
            print("  No image URL found on page")
            continue

        print(f"  Found image: {img_url}")

        try:
            download_image(img_url, out_path)
            print(f"  Saved to {out_path}")
        except Exception as e:
            print(f"  ERROR downloading image: {e}")

        time.sleep(REQUEST_DELAY_SECONDS)

    print("Done.")


if __name__ == "__main__":
    main()
