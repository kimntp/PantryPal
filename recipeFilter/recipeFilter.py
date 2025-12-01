import json

# IMPORTANT: ensure file paths are correct for your own device
RECIPES_FILE_PATH = "/Users/gh3work/code_projects/PantryPal/recipeFilter/all_recipes.json"
INGREDIENTS_FILE_PATH = "/Users/gh3work/code_projects/PantryPal/ingredientGetter/ingredients.json"
OUTPUT_FILE_PATH = "/Users/gh3work/code_projects/PantryPal/recipeFilter"

vegetarian_recipes = []
gluten_free_recipes = []
vegetarian_and_gluten_free = []

recipe_file = open(RECIPES_FILE_PATH, "r")
recipe_database = json.load(recipe_file)
ingredient_file = open(INGREDIENTS_FILE_PATH, "r")
ingredient_database = json.load(ingredient_file)

for recipe in recipe_database:
    recipe_ingredients = recipe["ingredients"]
    is_vegetarian = vegetarian(recipe_ingredients)
    is_gluten_free = gluten_free(recipe_ingredients)

    if is_vegetarian == True:
        vegetarian_recipes.append(recipe)
    
    if is_gluten_free == True:
        gluten_free_recipes.append(recipe)
    
    if is_vegetarian == True and is_gluten_free == True:
        vegetarian_and_gluten_free.append(recipe)

if recipe_file in locals() and not recipe_file.closed:
        recipe_file.close()
if ingredient_file in locals() and not ingredient_file.closed:
        ingredient_file.close()

def vegetarian(ingredient_list):
    for ingredient in ingredient_list:
        if ingredient_database[ingredient]["is_meat"] == True:
            return False
    return True

def gluten_free(ingredient_list):
    for ingredient in ingredient_list:
        if ingredient_database[ingredient]["has_gluten"] == True:
            return False
    return True
                                        
# dumps "vegetarian_recipes" list to a new vegetarian database called "vegetarian_recipes.json"
with open(OUTPUT_FILE_PATH + "/vegetarian_recipes.json", "w") as vegetarian_file:
    json.dump(vegetarian_recipes, vegetarian_file, indent=4)

# dumps "gluten_free_recipes" list to a new gluten-free database called "gluten_free_recipes.json"
with open(OUTPUT_FILE_PATH + "/gluten_free_recipes.json", "w") as gluten_free_file:
    json.dump(gluten_free_recipes, gluten_free_file, indent=4)

# dumps "vegetarian_and_gluten_free" to a new database called "vegetarian_and_gluten_free_recipes.json"
with open(OUTPUT_FILE_PATH + "/vegetarian_and_gluten_free_recipes.json", "w") as vgf_file:
    json.dump(vegetarian_and_gluten_free, vgf_file, indent=4)