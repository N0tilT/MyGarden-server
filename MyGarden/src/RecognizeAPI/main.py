from fastapi import FastAPI, UploadFile, File, Query # type: ignore
from fastapi.responses import JSONResponse # type: ignore
from fastapi.middleware.cors import CORSMiddleware # type: ignore
import shutil
from recognizer.identifier import identify
from data.plant_service import get_plants_data
app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.post("/recognize")
async def recognize_plant(image: UploadFile = File(...)):
    with open(f"uploads/{image.filename}", "wb") as buffer:
        shutil.copyfileobj(image.file, buffer)
        
    plants_data = get_plants_data()

    identifiers = identify([f"uploads/{image.filename}"],plants_data,top_number=3)
    for index,id in enumerate(identifiers):
        print(f"ФОТО {index+1} ({id[0]})")
        print('\n'.join([f"\t{x[0]:<50}\t\t{x[1]}" for x in id[1]]))

    result = {"filename": image.filename, "message": '\n'.join([f"\t{x[0]:<50}\t\t{x[1]}" for x in id[1]]), "species": "Example Species"}
    
    return JSONResponse(content=result)

@app.get("/search")
async def search_plant(name: str = Query(...)):
    
    plants_data = get_plants_data()
    result=[x for x in plants_data if name.lower() in ((x['title']).lower())]
    return JSONResponse(content=result)
    