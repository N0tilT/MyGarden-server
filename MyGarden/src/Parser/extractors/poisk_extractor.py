import json
from playwright.async_api import async_playwright

BASE_URL = 'https://onlinesemena.ru'
DRIVER_GET_TIMEOUT_MS = 60*10*1000

EXPECTED_PLANT_NUMBER = 1266
EXPECTED_FLOWER_NUMBER = 1734

catalog = './catalogues/poisk'
data_path = catalog + '/data'
links_path = catalog + '/links'
plant_json_filename = data_path + '/plant_data.json'
plant_links_filename = links_path + '/plant_links_data.txt'
error_plant_links_filename = links_path + '/error_plant_pages.txt'
flower_json_filename = data_path + '/flower_data.json'
flower_links_filename = links_path + '/flower_links_data.txt'
error_flower_links_filename = links_path + '/error_flower_pages.txt'

async def poisk_extract():
    await extract_plant()
    await flower_extract()

## PLANT EXTRACTING ###
async def extract_plant():
    plant_links = []
    plants_data = []
    with open(plant_links_filename,'r') as links_file:
        plant_links = links_file.read().split(',\n')
    # if len(plant_links) != EXPECTED_PLANT_NUMBER:
    #     plant_links = await extract_links_playwright(plant_links_filename,'semena-ovoshhej.html')
    print(f"PLANT LINK EXTRACTING DONE. got:{len(plant_links)}")
    with open(plant_json_filename,'r',encoding='utf-8') as json_file:
        plants_data = json.load(json_file) 
    if len(plants_data) != EXPECTED_PLANT_NUMBER:
        await extract_info_playwright(plant_links,plant_json_filename,error_plant_links_filename)
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
        flower_links = await extract_links_playwright(flower_links_filename,'catalog-semena-cvetov.html')
    print(f"FLOWER LINK EXTRACTING DONE. got:{len(flower_links)}")
    with open(flower_json_filename,'r',encoding='utf-8') as json_file:
        flower_data = json.load(json_file)
    if len(flower_data) != EXPECTED_FLOWER_NUMBER:
        await extract_info_playwright(flower_links,flower_json_filename,error_flower_links_filename)
        with open(flower_json_filename,'r',encoding='utf-8') as json_file:
            flower_data = json.load(json_file)
    print(f"FLOWER DATA EXTRACTING DONE: expected {EXPECTED_FLOWER_NUMBER} got {len(flower_data)}")


async def extract_links_playwright(links_filename: str,url:str):
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()

        catalog_url = f'{BASE_URL}/{url}'

        info_links = []
        page_number = 1
        while True:
            try:
                await page.goto(f"{catalog_url}/catalog?p={page_number}", timeout=DRIVER_GET_TIMEOUT_MS)
                links = await page.query_selector_all('div.filter_list a.name')
                current_links = [await item.get_attribute('href') for item in links]

                if page_number == 1:
                    info_links.extend(current_links)
                    page_number+=1
                    continue
                elif current_links == info_links[-len(current_links):]:
                    break

                info_links.extend(current_links)
                page_number += 1
            except Exception as e:
                print(f"Ошибка при обработке страницы {f"{catalog_url}?p={page_number}"}: {e}")
                break

        await browser.close()
        
        with open(links_filename, 'a', encoding='utf-8') as link_file:
            link_file.write(',\n'.join(info_links) + '\n')
        return info_links


async def extract_info_playwright(plant_info_links, json_filename,error_filename):
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()
        error_pages = []

        with open(json_filename, 'a', encoding='utf-8') as json_file:
            for index, plant_info_link in enumerate(plant_info_links):
                try:
                    await page.goto(f'{BASE_URL}{plant_info_link}', timeout=DRIVER_GET_TIMEOUT_MS)
                    print(f'{plant_info_link}')
                    title = await page.query_selector('h1.nom-name')
                    title_text = await title.inner_text() if title else 'Нет данных'
                    
                    div = await page.query_selector("div.desc")
                    description = await div.query_selector_all("p")
                    all_text = ""
                    for p in description:
                        class_name = await p.get_attribute("class")
                        if class_name != "anwer-agreement":
                            inner_html = await p.text_content()
                            all_text += inner_html + " " 
                    
                    plant_info = {
                        "title": title_text,
                        "description": all_text.replace('\xa0','').replace('Нажимая кнопку "Отправить", я даю согласие на обработку персональных данных','').strip(),
                    }
                    if title_text == "Нет данных":
                        raise Exception
                    json.dump(plant_info, json_file, ensure_ascii=False, indent=4)
                    json_file.write(',\n')
                except Exception as e:
                    error_pages.append(f'{plant_info_link}')
                    print(f"Ошибка при обработке страницы {f'{plant_info_link}'}: {e}")

        await browser.close()

        with open(error_filename, "w") as error_file:
            error_file.write(',\n'.join(error_pages))
