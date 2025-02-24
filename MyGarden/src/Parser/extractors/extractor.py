from ailita_extractor import ailita_extract
from poisk_extractor import poisk_extract
from altay_extractor import altay_extract
from partner_extractor import partner_extract
from gavrish_extractor import gavrish_extract
from data.plant_service import deduplicatePlantsData
import asyncio

catalog = 'gavrish'
image_path = catalog + './photos'
plant_json_filename = ''
flower_json_filename = ''

async def main():
    match catalog:
        case 'ailita':
            await ailita_extract()
        case 'poisk':
            await poisk_extract()
        case 'altay':
            await altay_extract()
        case 'partner':
            await partner_extract()
        case 'gavrish':
            await gavrish_extract()
    deduplicatePlantsData(['ailita','poisk','altay','partner','gavrish'])
asyncio.run(main())



