import json

ingredients_list = []
is_meat = []
has_gluten = []

# fetches the individual ingredients from each recipe ignoring duplicates
with open("/Users/gh3work/code_projects/PantryPal/ingredientGetter/allRecipes.json", "r") as dataset:
    recipe_list = json.load(dataset)
    for recipe in recipe_list:
        ingredients = recipe['ingredients']
        for ingredient in ingredients:
            if ingredient not in ingredients_list:
                ingredients_list.append(ingredient)
            else:
                continue

i = 0
while(i < 30):
    meat = input("Is " + ingredients_list[i] + " a meat? [y/n]  ")
    gluten = input("Does " + ingredients_list[i] + " contain gluten? [y/n]  ")

    if(meat == 'y'):
        is_meat.append(ingredients_list[i])
    if(gluten == "y"):
        has_gluten.append(ingredients_list[i])

    i+=1
    
# Adds each unique indvidual ingredient to a new database called "ingredients.txt"
with open("/Users/gh3work/code_projects/PantryPal/ingredientGetter/all_ingredients.txt", "w") as all_ingredients:
    for ingredient in ingredients_list:
        print(ingredient, file=all_ingredients)

# Adds each meat in "meats" to a new database called "meats.txt"
with open("/Users/gh3work/code_projects/PantryPal/ingredientGetter/meats.txt", "w") as meats:
    for meat in is_meat:
        print(meat, file=meats)

# Adds each gluten in "glutens" to a new database called "glutens.txt"
with open("/Users/gh3work/code_projects/PantryPal/ingredientGetter/glutens.txt", "w") as glutens:
    for gluten in has_gluten:
        print(gluten, file=glutens)