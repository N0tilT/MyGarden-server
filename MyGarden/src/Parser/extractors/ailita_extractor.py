import json
from playwright.async_api import async_playwright

EXPECTED_PLANT_NUMBER = 3075-66
EXPECTED_FLOWER_NUMBER = 2563

BASE_URL = 'https://www.ailita.ru/catalog'
DRIVER_GET_TIMEOUT_MS = 60*10*1000

catalog = '../catalogues/ailita'
data_path = catalog + '/data'
links_path = catalog + '/links'
plant_json_filename = data_path + '/plant_data.json'
plant_links_filename = links_path + '/plant_links_data.txt'
error_plant_links_filename = links_path + '/error_plant_pages.txt'
flower_json_filename = data_path + '/flower_data.json'
flower_links_filename = links_path + '/flower_links_data.txt'
error_flower_links_filename = links_path + '/error_flower_pages.txt'

async def ailita_extract():
    await extract_plant()
    await flower_extract()

## PLANT EXTRACTING ###
async def extract_plant():
    plant_links = []
    plants_data = []
    with open(plant_links_filename,'r') as links_file:
        plant_links = links_file.read().split('\n')
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


### FLOWER EXTRACTING ###
async def flower_extract():
    flower_links = []
    flower_data = []
    with open(flower_links_filename,'r') as links_file:
        flower_links = links_file.read().split(',\n')
    if len(flower_links) != EXPECTED_FLOWER_NUMBER:
        catalogues = [ f'{BASE_URL}/dvuletnie-tsvety/','https://www.ailita.ru/catalog/mnogoletnie-tsvety/',f'{BASE_URL}/odnoletnie-tsvety2874/']
        flower_links = extract_flower_links_playwright(catalogues,error_flower_links_filename)
    print(f"FLOWER LINK EXTRACTING DONE. got:{len(flower_links)}")
    with open(flower_json_filename,'r',encoding='utf-8') as json_file:
        flower_data = json.load(json_file)
    if len(flower_data) != EXPECTED_FLOWER_NUMBER:
        await extract_flower_info_playwright(flower_links,flower_json_filename,error_flower_links_filename)
        with open(flower_json_filename,'r',encoding='utf-8') as json_file:
            flower_data = json.load(json_file)
    print(f"FLOWER DATA EXTRACTING DONE: expected {EXPECTED_FLOWER_NUMBER} got {len(flower_data)}")


async def extract_plant_links_playwright(links_filename: str):
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()

        catalog_url = f'{BASE_URL}/standartnaya-seriya2725/'
        await page.goto(catalog_url)

        plant_links = await page.query_selector_all('ul.catalog-sections__list li.catalog-sections__item a')
        plant_links = [await link.get_attribute('href') for link in plant_links]

        plant_info_links = []
        for link in plant_links:
            page_number = 1
            while True:
                try:
                    await page.goto(f"{link}?PAGEN_1={page_number}", timeout=DRIVER_GET_TIMEOUT_MS)
                    item_links = await page.query_selector_all('a.catalog-item2__link')
                    current_links = [await item.get_attribute('href') for item in item_links]

                    if page_number == 1:
                        plant_info_links.extend(current_links)
                    elif current_links == plant_info_links[-len(current_links):]:
                        break

                    plant_info_links.extend(current_links)
                    page_number += 1
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
                    await page.goto(plant_info_link, timeout=DRIVER_GET_TIMEOUT_MS)
                    print(plant_info_link)
                    title = await page.query_selector('h1.product-detail__title')
                    title_text = await title.inner_text() if title else 'Нет данных'
                    
                    description = await page.query_selector("div.product-detail__cont.product-detail__description")
                    description_text = await description.inner_text() if description else 'Нет данных'
                    
                    additional_info = await page.query_selector_all("div.product-detail__cont:not(.product-detail__description)")
                    additional_texts = [await info.inner_text() for info in additional_info]

                    program_titles = await page.query_selector_all("div.product-program__item-title")
                    program_texts = [await title.inner_text() for title in program_titles]

                    plant_info = {
                        "title": title_text,
                        "description": description_text,
                        "additional_info": additional_texts,
                        "programs": program_texts
                    }
                    if title_text == "Нет данных":
                        raise Exception
                    json.dump(plant_info, json_file, ensure_ascii=False, indent=4)
                    json_file.write(',\n')
                except Exception as e:
                    error_pages.append(plant_info_link)
                    print(f"Ошибка при обработке страницы {plant_info_link}: {e}")

        await browser.close()

        with open(error_filename, "w") as error_file:
            error_file.write(',\n'.join(error_pages))


async def extract_flower_links_playwright(catalogues, links_filename: str):
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        total_flower_info_links = []

        for catalog_url in catalogues:
            print(catalog_url)
            current_flower_info_links = []
            page = await browser.new_page()
            await page.goto(catalog_url)

            try:
                flower_links_elements = await page.query_selector_all('ul.catalog-sections__list > li > a.catalog-section-item__outer.link')
                flower_links = [await element.get_attribute('href') for element in flower_links_elements]
            except Exception as e:
                print(f"Ошибка при парсинге каталога {catalog_url}: {e}")
                continue

            for link in flower_links:
                page_number = 1
                while True:
                    try:
                        await page.goto(f"{link}?PAGEN_1={page_number}", timeout=DRIVER_GET_TIMEOUT_MS * 10)
                        item_links = await page.query_selector_all('a.catalog-item2__link')
                        current_links = [await item.get_attribute('href') for item in item_links]

                        if page_number == 1:
                            current_flower_info_links.extend(current_links)
                        elif current_links == current_flower_info_links[-len(current_links):]:
                            break

                        current_flower_info_links.extend(current_links)
                        page_number += 1
                    except Exception as e:
                        print(f"Ошибка при обработке страницы {link}: {e}")
                        break

            total_flower_info_links.extend(current_flower_info_links)
            with open(links_filename, 'a', encoding='utf-8') as link_file:
                link_file.write(',\n'.join(current_flower_info_links) + '\n')

        await browser.close()
        return total_flower_info_links


async def extract_flower_info_playwright(plant_info_links, json_filename,error_filename):
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()
        error_pages = []

        with open(json_filename, 'a', encoding='utf-8') as json_file:
            for index, plant_info_link in enumerate(plant_info_links):
                try:
                    await page.goto(plant_info_link, timeout=DRIVER_GET_TIMEOUT_MS * 10)
                    print(plant_info_link)
                    title_element = await page.query_selector("h1.product-detail__title")
                    title_text = await title_element.inner_text() if title_element else 'Нет данных'

                    description_element = await page.query_selector("div.product-detail__cont.product-detail__description[itemprop='description']")
                    description_text = await description_element.inner_text() if description_element else 'Нет данных'

                    additional_info_elements = await page.query_selector_all("div.product-detail__cont:not(.product-detail__description)")
                    additional_texts = [await info.inner_text() for info in additional_info_elements]

                    program_titles_elements = await page.query_selector_all("div.product-program__item-title")
                    program_texts = [await title.inner_text() for title in program_titles_elements]

                    plant_info = {
                        "title": title_text,
                        "description": description_text,
                        "additional_info": additional_texts,
                        "programs": program_texts
                    }

                    if title_text == "Нет данных":
                        raise Exception
                    
                    json.dump(plant_info, json_file, ensure_ascii=False, indent=4)
                    json_file.write(',\n')

                except Exception as e:
                    error_pages.append(plant_info_link)
                    print(f"Ошибка при обработке страницы {plant_info_link}: {e}")

        await browser.close()

        with open(error_filename, "w") as error_file:
            error_file.write(',\n'.join(error_pages))