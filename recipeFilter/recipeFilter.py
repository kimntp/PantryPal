import json

RECIPES_FILE_PATH = "PPApp/Resources/Raw/cleanedDataset.json"
INGREDIENTS_FILE_PATH = "PPApp/Resources/Raw/ingredients.json"
OUTPUT_FILE_PATH = "PPApp/Resources/Raw/ingredientIndex.json"

recipe_list = {}

# loads relevant JSONs
with open(RECIPES_FILE_PATH, "r") as recipe_file:
    recipes = json.load(recipe_file)

with open(INGREDIENTS_FILE_PATH,"r") as ingredient_file:
    ingredients = json.load(ingredient_file)
         
# builds dictionary in the form
# { ingredient_name : {is_meat: ..., has_gluten: ..., recipes: [ ... ]}
for ingredient_obj in ingredients:
    ingredient_name = ingredient_obj["ingredient"]
    is_meat = ingredient_obj["is_meat"]
    has_gluten = ingredient_obj["has_gluten"]

    recipes_containing_ingredient = []
    
    for recipe in recipes:
        # recipe["ingredients"] is a list of strings
        if ingredient_name in recipe["ingredients"]:
            recipes_containing_ingredient.append(recipe)

    # formats and adds ingredient and relevant info to "recipe_list"
    recipe_list[ingredient_name] = {
        "is_meat": is_meat,
        "has_gluten": has_gluten,
        "recipes": recipes_containing_ingredient
    }

# dumps "recipe_list" to a tagged recipe database called "ingredientIndex.json"
with open(OUTPUT_FILE_PATH, "w") as output_file:
    json.dump(recipe_list, output_file, indent=4)