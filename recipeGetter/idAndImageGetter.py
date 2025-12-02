import json
import re
import unicodedata

# INPUT
INPUT_PATH = "PPApp/Resources/Raw/cleanedDataset.json"

# OUTPUT
OUTPUT_PATH = "PPApp/Resources/Raw/finalizedDataset.json" 

def slugify(text: str) -> str:
    # normalize unicode → ASCII
    text = unicodedata.normalize("NFKD", text)
    text = text.encode("ascii", "ignore").decode("ascii")

    # lowercase
    text = text.lower().strip()

    # replace any non-alphanumeric characters with hyphens
    text = re.sub(r"[^a-z0-9]+", "-", text)

    # remove any leading/trailing hyphens
    text = text.strip("-")

    return text

def main():
    with open(INPUT_PATH,"r", encoding="utf-8") as f:
        recipes = json.load(f)

    image_overrides = {}

    # default image if none is specified
    DEFAULT_IMAGE_URL = "https://static.vecteezy.com/system/resources/thumbnails/073/584/268/small/no-food-smart-trendy-cute-amazing-design-useful-illustration-colorful-background-vector.jpg"

    for recipe in recipes:
        name = recipe.get("name", "").strip()

        # set recipeID to be slugified recipe name
        recipe_id = slugify(name)
        recipe["recipeID"] = recipe_id

        override_url = image_overrides.get(recipe_id)

        if override_url:
            recipe["imageUrl"] = override_url
        else:
            current = recipe.get("imageUrl")
            if not current:
                recipe["imageUrl"] = DEFAULT_IMAGE_URL


    with open(OUTPUT_PATH,"w", encoding="utf-8") as f:
        json.dump(recipes, f, indent=4, ensure_ascii=False)

    print(f"Updated {len(recipes)} recipes written to {OUTPUT_PATH}")

if __name__ == "__main__":
    main()
