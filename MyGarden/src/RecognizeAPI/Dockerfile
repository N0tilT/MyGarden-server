FROM python:3.12-slim

RUN apt-get update && apt-get -y install tesseract-ocr \
    tesseract-ocr-rus \
    curl \
    libgl1 \
    libglib2.0-0 \
    libsm6 \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 80

WORKDIR /src
COPY ["src/RecognizeAPI/", "./"]

RUN pip install --no-cache-dir -r requirements.txt

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "80"]