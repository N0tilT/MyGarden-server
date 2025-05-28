from fastapi import FastAPI, UploadFile, File, Query  # type: ignore
from fastapi.responses import JSONResponse  # type: ignore
from fastapi.middleware.cors import CORSMiddleware  # type: ignore
import shutil
from recognizer.identifier import identify
from data.plant_service import get_plants_data,get_prefill_data
from prometheus_fastapi_instrumentator import Instrumentator
import logging
import json_log_formatter

formatter = json_log_formatter.JSONFormatter()
handler = logging.FileHandler('/var/log/fastapi.log')
handler.setFormatter(formatter)
logger = logging.getLogger('uvicorn')
logger.addHandler(handler)

app = FastAPI()

Instrumentator().instrument(app).expose(app)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

plants_data = get_plants_data()


@app.post("/recognize")
async def recognize_plant(image: UploadFile = File(...)):
    with open(f"uploads/{image.filename}", "wb") as buffer:
        shutil.copyfileobj(image.file, buffer)
    identifiers = identify(
        [f"uploads/{image.filename}"], plants_data, top_number=3)
    print(identifiers)
    for index, id in enumerate(identifiers):
        print(f"ФОТО {index+1} ({id[0]})")
        print('\n'.join([f"{x[0]}\t{x[1]:<50}\t\t{x[2]}" for x in id[1]]))
    result = {"filename": image.filename, "message": '\n'.join(
        [f"\t{x[0]:<50}\t\t{x[1]}" for x in id[1]]), "species": "Example Species"}

    return JSONResponse(content=result)


@app.get("/search")
async def search_plant(name: str = Query(...)):
    result = [x for x in plants_data if name.lower() in ((x['title']).lower())]
    return JSONResponse(content=result)


@app.get("/prefill")
async def prefill_data(type: str = Query(...)):
    default_response = {
        "type": "",
        "articles": [],
        "labels": {
            "WateringNeed": "",
            "LightNeed": "",
            "Fertilizer": []
        },
        "summary": ""
    }
    
    result = get_prefill_data(type)
    
    if not result:
        return JSONResponse(content=default_response)
    
    response = {
        "type": result["type"],
        "articles": result["articles"],
        "labels": {
            "WateringNeed": result["labels"].get("WateringNeed", ""),
            "LightNeed": result["labels"].get("LightNeed", ""),
            "Fertilizer": result["labels"].get("Fertilizer", [])
        },
        "summary": result["summary"]
    }
    
    return JSONResponse(content=response)


@app.get("/health")
def health_check():
    return {"status": "healthy"}
