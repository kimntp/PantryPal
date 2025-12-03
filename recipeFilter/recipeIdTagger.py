import json
import re
import unicodedata

# INPUT
INPUT_PATH = "PPApp/Resources/Raw/cleanedDataset.json"

# OUTPUT
OUTPUT_PATH = "PPApp/Resources/Raw/finalizedDataset.json" 

def main():
    with open(INPUT_PATH,"r", encoding="utf-8") as f:
        recipes = json.load(f)

    image_overrides = {}

    # default image if none is specified
    DEFAULT_IMAGE_URL = "https://static.vecteezy.com/system/resources/thumbnails/073/584/268/small/no-food-smart-trendy-cute-amazing-design-useful-illustration-colorful-background-vector.jpg"

    recipe_index = 0
    for recipe in recipes:
        recipe["recipeID"] = recipe_index
        recipe_index +=1
    
    with open(OUTPUT_PATH,"w", encoding="utf-8") as f:
        json.dump(recipes, f, indent=4, ensure_ascii=False)

    print(f"Updated {len(recipes)} recipes written to {OUTPUT_PATH}")

if __name__ == "__main__":
    main()
