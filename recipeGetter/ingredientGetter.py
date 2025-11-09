import json

ingredients_list = []

# fetches the individual ingredients from each recipe in the clean recipe dataset 
# and adds them to a new database called "ingredients.txt"

with open("/Users/gh3work/code_projects/PantryPal/recipeGetter/cleanRecipes.json", "r") as dataset:
    recipe_list = json.load(dataset)
    for recipe in recipe_list:
        ingredients = recipe['ingredients']
        for ingredient in ingredients:
            if ingredient not in ingredients_list:
                ingredients_list.append(ingredient)
            else:
                    continue

with open("/Users/gh3work/code_projects/PantryPal/recipeGetter/ingredients.txt", "w") as output_file:
    for ingredient in ingredients_list:
        print(ingredient, file=output_file)