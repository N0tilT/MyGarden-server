import json
import os
import asyncio
from playwright.async_api import async_playwright

EXPECTED_PLANT_NUMBER = 2322
EXPECTED_FLOWER_NUMBER = 4870
BASE_URL = 'https://stroy-podskazka.ru/knowledge'
DRIVER_GET_TIMEOUT_MS = 60*10*1000

catalog = 'C:/Users/timofey.latypov/Documents/MyGarden-server/MyGarden/src/Parser/catalogues/stroy_podskazka'
data_path = catalog + '/data'
links_path = catalog + '/links'
plant_json_filename = data_path + '/plant_data.json'
plant_types_json_filename = data_path + '/plant_types.json'
plant_articles_json_filename = data_path + '/plant_articles.json'
plant_links_filename = links_path + '/plant_links_data.txt'
error_plant_links_filename = links_path + '/error_plant_pages.txt'
flower_json_filename = data_path + '/flower_data.json'
flower_links_filename = links_path + '/flower_links_data.txt'
flower_types_json_filename = data_path + '/flower_types.json'
flower_articles_json_filename = data_path + '/flower_articles.json'
error_flower_links_filename = links_path + '/error_flower_pages.txt'

async def podskazka_extract():
    await extract_plant()
    await flower_extract()

## PLANT EXTRACTING ###
async def extract_plant():
    plant_links = []
    plants_data = []
    with open(plant_links_filename,'r') as links_file:
        plant_links = links_file.read().split(',\n')
    if len(plant_links) != EXPECTED_PLANT_NUMBER:
        plant_links = await extract_plant_links_playwright(plant_links_filename)
    print(f"PLANT LINK EXTRACTING DONE. got:{len(plant_links)}")
    with open(plant_json_filename,'r',encoding='utf-8') as json_file:
        plants_data = json.load(json_file) 
    if len(plants_data) != EXPECTED_PLANT_NUMBER:
        await extract_plants_info_playwright(plant_links,plant_json_filename,error_plant_links_filename)
        # extract_plants_info(plant_links,plant_json_filename,options)
        with open(plant_json_filename,'r',encoding='utf-8') as json_file:
            plants_data = json.load(json_file) 
    print(f"PLANT DATA EXTRACTING DONE: expected {EXPECTED_PLANT_NUMBER} got {len(plants_data)}")
    #Plant types
    types = set([l.split("/")[1] for l in plant_links])
    types_json = [{"id":index,"title":link} for index,link in enumerate(types)]
    with open(plant_types_json_filename,'r',encoding='utf-8') as json_file:
        if (len(json.load(json_file))!=len(types)):
            with open(plant_types_json_filename,'w',encoding='utf-8') as json_file:
                json.dump(types_json,json_file, ensure_ascii=False, indent=4)
    #Plant articles
    
    with open(plant_articles_json_filename,'r',encoding='utf-8') as json_file:
        if (len(json.load(json_file))!=len(types)):
            await extract_articles(types,plant_articles_json_filename)

### FLOWER EXTRACTING ###
async def flower_extract():
    flower_links = []
    flower_data = []
    with open(flower_links_filename,'r') as links_file:
        flower_links = links_file.read().split(',\n')
    if len(flower_links) != EXPECTED_FLOWER_NUMBER:
        flower_links = await extract_flower_links_playwright(flower_links_filename)
    print(f"FLOWER LINK EXTRACTING DONE. got:{len(flower_links)}")
    with open(flower_json_filename,'r',encoding='utf-8') as json_file:
        flower_data = json.load(json_file)
    if len(flower_data) != 4886:
        await extract_flower_info_playwright(flower_links,flower_json_filename,error_flower_links_filename)
        with open(flower_json_filename,'r',encoding='utf-8') as json_file:
            flower_data = json.load(json_file)
    print(f"FLOWER DATA EXTRACTING DONE: expected {EXPECTED_FLOWER_NUMBER} got {len(flower_data)}")
    #Flower types
    types = set([l.split("/")[1] for l in flower_links])
    types_json = [{"id":index,"title":link} for index,link in enumerate(types)]
    with open(flower_types_json_filename,'r',encoding='utf-8') as json_file:
        if (len(json.load(json_file))!=len(types)):
            with open(flower_types_json_filename,'w',encoding='utf-8') as json_file:
                json.dump(types_json,json_file, ensure_ascii=False, indent=4)
    #Flower articles
    with open(flower_articles_json_filename,'r',encoding='utf-8') as json_file:
        if (len(json.load(json_file))!=len(types)):
            await extract_articles(types,flower_articles_json_filename)

async def extract_articles(types:list[str],links_filename: str):
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()

        base = f'{BASE_URL.rsplit('/',1)[0]}'
        links = [f'{base}/{type_title}' for type_title in types]
        item_articles = []
        for index,item in enumerate(zip(links,types)):
            try:
                link=item[0]
                type=item[1]
                await page.goto(link, timeout=DRIVER_GET_TIMEOUT_MS)
                articles = await page.query_selector_all('a.box.box-news')
                titles = [await article.query_selector('span.box-name') for article in articles]
                current_links = list(zip([f'{base}{await item.get_attribute('href')}' for item in articles],
                                         [await item.text_content() for item in titles]))
                articles = await page.query_selector_all('a.box.box-article')
                titles = [await article.query_selector('span.box-name') for article in articles]
                current_links.extend(list(zip([f'{base}{await item.get_attribute('href')}' 
                                               for item in articles],[await item.text_content() for item in titles])))

                item_articles.append({"id":index,"type":type,"articles":[{"title":l[1],"link":l[0]} for l in current_links]})
            except Exception as e:
                print(f"Ошибка при обработке страницы {link}: {e}")
                break

        await browser.close()
        
        with open(links_filename, 'a', encoding='utf-8') as json_file:
            json.dump(item_articles, json_file, ensure_ascii=False, indent=4)
        return item_articles
    
async def extract_plant_links_playwright(links_filename: str):
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()

        catalog_url = f'{BASE_URL}'
        await page.goto(catalog_url)
        
        divs = await page.query_selector_all('div.wrapper')
        links = [await div.query_selector_all('a') for div in divs]
        
        plant_links = [await link.get_attribute('href') for link in links[1]]
    
        plant_info_links = []
        for link in plant_links:
            try:
                await page.goto(f"{BASE_URL.rsplit('/',1)[0]}{link}", timeout=DRIVER_GET_TIMEOUT_MS)
                item_links = await page.query_selector_all('a.gallery_item.knowledge_item')
                current_links = [await item.get_attribute('href') for item in item_links]

                plant_info_links.extend(current_links)
            except Exception as e:
                print(f"Ошибка при обработке страницы {link}: {e}")
                break

        await browser.close()
        
        with open(links_filename, 'a', encoding='utf-8') as link_file:
            link_file.write(',\n'.join(plant_info_links) + '\n')
        return plant_info_links


async def extract_plants_info_playwright(plant_info_links, json_filename,error_filename):
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()
        error_pages = []

        with open(json_filename, 'a', encoding='utf-8') as json_file:
            for index, plant_info_link in enumerate(plant_info_links):
                try:
                    await page.goto(f"{BASE_URL.rsplit('/',1)[0]}{plant_info_link}#specification", timeout=DRIVER_GET_TIMEOUT_MS)
                    print(plant_info_link)
                    result = {"id":index,"link":plant_info_link}
                    categories = await page.query_selector_all('div > div:has(div[style*="font-weight: bold"])')
                    
                    for category in categories:
                        category_name =(await (await category.query_selector('div[style*="font-weight: bold"]')).inner_text()).strip()
                        
                        properties = {}
                        dl_elements = await category.query_selector_all('dl')
                        
                        for dl in dl_elements:
                            dt = await dl.query_selector('dt')
                            dt_text =(await (await  dt.query_selector('.dt_inner_class')).inner_text()).strip() if dt else ''
                            dd =(await (await dl.query_selector('dd')).inner_text()).strip() if await dl.query_selector('dd') else ''
                            
                            properties[dt_text]= dd
                        
                        result[category_name] = properties

                    await page.goto(f"{BASE_URL.rsplit('/',1)[0]}{plant_info_link}#tab_description", timeout=DRIVER_GET_TIMEOUT_MS)
                    article =await  page.query_selector('article.article.article_entry')
    
                    if not article:
                        return None
                    
                    
                    h2_elements =await article.query_selector_all('h2')
                    
                    for h2 in h2_elements:
                        property_name =(await h2.inner_text()).strip()
                        div =await h2.query_selector('xpath=following-sibling::div[1]')
                        
                        if div:
                            property_value = (await div.inner_text()).strip()
                            result[property_name] = property_value
                    json.dump(result, json_file, ensure_ascii=False, indent=4)
                    json_file.write(',\n')
                except Exception as e:
                    error_pages.append(plant_info_link)
                    print(f"Ошибка при обработке страницы {plant_info_link}: {e}")

        await browser.close()

        with open(error_filename, "w") as error_file:
            error_file.write(',\n'.join(error_pages))


async def extract_flower_links_playwright(links_filename: str):
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()

        catalog_url = f'{BASE_URL}'
        await page.goto(catalog_url)
        
        divs = await page.query_selector_all('div.wrapper')
        links = [await div.query_selector_all('a') for div in divs]
        
        plant_links = [await link.get_attribute('href') for link in links[3]]
    
        plant_info_links = []
        for link in plant_links:
            try:
                await page.goto(f"{BASE_URL.rsplit('/',1)[0]}{link}", timeout=DRIVER_GET_TIMEOUT_MS)
                item_links = await page.query_selector_all('a.gallery_item.knowledge_item')
                current_links = [await item.get_attribute('href') for item in item_links]

                plant_info_links.extend(current_links)
            except Exception as e:
                print(f"Ошибка при обработке страницы {link}: {e}")
                break

        await browser.close()
        
        with open(links_filename, 'a', encoding='utf-8') as link_file:
            link_file.write(',\n'.join(plant_info_links) + '\n')
        return plant_info_links


async def extract_flower_info_playwright(plant_info_links, json_filename,error_filename):
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()
        error_pages = []

        with open(json_filename, 'a', encoding='utf-8') as json_file:
            for index, plant_info_link in enumerate(plant_info_links,start=4886):
                try:
                    await page.goto(f"{BASE_URL.rsplit('/',1)[0]}{plant_info_link}#specification", timeout=DRIVER_GET_TIMEOUT_MS)
                    print(plant_info_link)
                    result = {"id":index,"link":plant_info_link}
                    categories = await page.query_selector_all('div > div:has(div[style*="font-weight: bold"])')
                    
                    for category in categories:
                        category_name =(await (await category.query_selector('div[style*="font-weight: bold"]')).inner_text()).strip()
                        
                        properties = []
                        dl_elements = await category.query_selector_all('dl')
                        
                        for dl in dl_elements:
                            dt = await dl.query_selector('dt')
                            dt_text =(await (await  dt.query_selector('.dt_inner_class')).inner_text()).strip() if dt else ''
                            dd =(await (await dl.query_selector('dd')).inner_text()).strip() if await dl.query_selector('dd') else ''
                            
                            properties.append({dt_text: dd})
                        
                        result[category_name] = properties

                    await page.goto(f"{BASE_URL.rsplit('/',1)[0]}{plant_info_link}#tab_description", timeout=DRIVER_GET_TIMEOUT_MS)
                    article =await  page.query_selector('article.article.article_entry')
    
                    if not article:
                        return None
                    
                    
                    h2_elements =await article.query_selector_all('h2')
                    
                    for h2 in h2_elements:
                        property_name =(await h2.inner_text()).strip()
                        div =await h2.query_selector('xpath=following-sibling::div[1]')
                        
                        if div:
                            property_value = (await div.inner_text()).strip()
                            result[property_name] = property_value
                    json.dump(result, json_file, ensure_ascii=False, indent=4)
                    json_file.write(',\n')
                except Exception as e:
                    error_pages.append(plant_info_link)
                    print(f"Ошибка при обработке страницы {plant_info_link}: {e}")

        await browser.close()

        with open(error_filename, "w") as error_file:
            error_file.write(',\n'.join(error_pages))

asyncio.run(podskazka_extract())