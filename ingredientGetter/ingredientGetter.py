import json

# IMPORTANT: replace the file paths below with the appropriate locations on your own computer
# if you want to run the program on your own computer 

#input file
RECIPES_FILE_PATH = "/Users/gh3work/code_projects/PantryPal/recipeFilter/all_recipes.json"
#output_file
INGREDIENTS_FILE_PATH = "/Users/gh3work/code_projects/PantryPal/ingredientGetter/ingredients.json"

ingredients_list = []
staged_ingredients = []

# fetches the individual ingredients from each recipe ignoring duplicates and adds to "ingredients_list"
with open(RECIPES_FILE_PATH, "r") as dataset:
    recipe_list = json.load(dataset)
    for recipe in recipe_list:
        ingredients = recipe['ingredients']
        for ingredient in ingredients:
            if ingredient not in ingredients_list:
                ingredients_list.append(ingredient)
            else:
                continue

i = 0
is_meat = False
has_gluten = False

# asks user if each ingredient in "ingredients_list" has meat or gluten and adds the ingredient
# to "staged_ingredients" along with the appopriate tags
for ingredient in ingredients_list:
    meat = input("Is " + ingredients_list[i] + " a meat? [y/n]  ")
    gluten = input("Does " + ingredients_list[i] + " contain gluten? [y/n]  ")
    
    if(meat == 'y'):
        is_meat = True
    if(gluten == 'y'):
        has_gluten = True
    
    formatted_ingredient = {
        "ingredient": ingredients_list[i],
        "is_meat": is_meat,
        "has_gluten": has_gluten
    }

    staged_ingredients.append(formatted_ingredient)
    if(is_meat == True):
        is_meat = False
    if(has_gluten == True):
        has_gluten = False
    
    i+=1                                                                                       

# dumps "staged_ingredients" to a tagged ingredient database called "ingredients.json"
with open(INGREDIENTS_FILE_PATH, "w") as ingredients_file:
    json.dump(staged_ingredients, ingredients_file, indent=4)