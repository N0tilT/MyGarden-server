FROM python:3.12-slim

# Установка Tesseract
RUN apt-get update && apt-get -y install\
    tesseract-ocr \
    && apt-get install tesseract-ocr-rus \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*


WORKDIR /app
COPY . .

# /usr/local/share/tessdata
RUN pip install --no-cache-dir -r requirements.txt
EXPOSE 80

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "80"]
