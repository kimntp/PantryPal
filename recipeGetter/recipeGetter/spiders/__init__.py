# This package will contain the spiders of your Scrapy project
#
# Please refer to the documentation for information on how to create and manage
# your spiders.
import scrapy

class bbcSpider(scrapy.Spider):
    name = "bbc2"
    start_urls = ["https://www.bbcgoodfood.com/recipes/collection/quick-and-easy-family-recipes?page=1",
                  "https://www.bbcgoodfood.com/recipes/collection/quick-and-easy-family-recipes?page=2"]

    def parse(self, response):
        # Your parsing logic here
        for recipeContent in response.css('div.card__section.card__content'):

            name = recipeContent.css('h2.heading-4::text').get()
            links = recipeContent.css('a.link.d-block::attr(href)').get()

            if name is None:
                continue

            if links:
                yield response.follow(links, callback=self.parse_recipe, meta={'name': name, 'links': links})
            #yield{
               # 'name': recipeContent.css('h2.heading-4::text').get(),
               # 'links': recipeContent.css('a.link.d-block').attrib['href'] 
           #}

        current_page = int(response.url.split('=')[-1])
        next_page = f"https://www.bbcgoodfood.com/recipes/collection/quick-and-easy-family-recipes?page={current_page + 1}"
        if next_page is not None:
            yield scrapy.Request(next_page, callback=self.parse)

    def parse_recipe(self, response):
        name = response.meta['name']
        links = response.meta['links']
        ingredients = response.css('span.ingredients-list__item-ingredient::text').getall()
       # ingredients = [i.strip() for i in ingredients if i.strip()]

        yield {
            'name': name,
            'url': response.url,
            'ingredients': ingredients 
        }