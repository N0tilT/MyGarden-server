import sqlite3

def create_tables(conn):
    with conn:
        conn.execute('''
        CREATE TABLE IF NOT EXISTS type (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT NOT NULL,
            category TEXT NOT NULL CHECK(category IN ('plant', 'flower')),
            UNIQUE(title, category)  -- Предотвращаем дубликаты
        )''')
        
        conn.execute('''
        CREATE TABLE IF NOT EXISTS prefill (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            type_id INTEGER NOT NULL,
            category TEXT NOT NULL CHECK(category IN ('plant', 'flower')),
            articles TEXT,
            labels TEXT,
            summary TEXT,
            FOREIGN KEY (type_id) REFERENCES type(id)
        )''')
        
def migrate_data(conn):
    with conn:
        cursor = conn.cursor()
        
        cursor.execute("SELECT title FROM plant_type")
        for (plant_type,) in cursor.fetchall():
            cursor.execute(
                "INSERT OR IGNORE INTO type (title, category) VALUES (?, 'plant')",
                (plant_type,)
            )
        
        cursor.execute("SELECT title FROM flower_type")
        for (flower_type,) in cursor.fetchall():
            cursor.execute(
                "INSERT OR IGNORE INTO type (title, category) VALUES (?, 'flower')",
                (flower_type,)
            )
        
        type_map = {}
        cursor.execute("SELECT id, title, category FROM type")
        for row in cursor.fetchall():
            type_map[(row[1], row[2])] = row[0]
        
        cursor.execute("SELECT type, articles, labels, summary FROM plant_prefill")
        for plant in cursor.fetchall():
            type_id = type_map.get((plant[0], 'plant'))
            if type_id:
                cursor.execute(
                    "INSERT INTO prefill (type_id, category, articles, labels, summary) VALUES (?, 'plant', ?, ?, ?)",
                    (type_id, plant[1], plant[2], plant[3])
                )
        
        # 5. Перенос данных цветов
        cursor.execute("SELECT type, articles, labels, summary FROM flower_prefill")
        for flower in cursor.fetchall():
            type_id = type_map.get((flower[0], 'flower'))
            if type_id:
                cursor.execute(
                    "INSERT INTO prefill (type_id, category, articles, labels, summary) VALUES (?, 'flower', ?, ?, ?)",
                    (type_id, flower[1], flower[2], flower[3])
                )
              
def remove_duplicates_in_prefill(conn):
    """Удаляет дубликаты с использованием оконных функций"""
    with conn:
        cursor = conn.cursor()
        
        cursor.execute('''
        CREATE TEMP TABLE ranked_prefill AS
        SELECT 
            id,
            ROW_NUMBER() OVER (
                PARTITION BY type_id, category 
                ORDER BY id
            ) AS rn
        FROM prefill
        ''')
        
        cursor.execute('''
        DELETE FROM prefill
        WHERE id IN (
            SELECT id 
            FROM ranked_prefill 
            WHERE rn > 1
        )
        ''')
        
        deleted_count = cursor.rowcount
        print(f"Удалено дубликатов: {deleted_count}")
        
        cursor.execute("DROP TABLE IF EXISTS ranked_prefill")
        
        return deleted_count
    
def main():
    conn = sqlite3.connect('prefill.db')
    
    create_tables(conn)
    
    migrate_data(conn)
    
    removed_count = remove_duplicates_in_prefill(conn)
    print(f"Удалено дубликатов: {removed_count}")
    
    verify_migration(conn)
    cursor = conn.cursor()
    cursor.execute('''DROP TABLE IF EXISTS plant_prefill;''')
    cursor.execute('''DROP TABLE IF EXISTS flower_prefill;''')
    cursor.execute('''DROP TABLE IF EXISTS plant_type;''')
    cursor.execute('''DROP TABLE IF EXISTS flower_type;''')
    conn.close()

def verify_migration(conn):
    """Проверка целостности после миграции"""
    cursor = conn.cursor()
    
    cursor.execute('''
    SELECT 
        type_id, 
        category, 
        COUNT(*) as cnt
    FROM prefill
    GROUP BY type_id, category
    HAVING cnt > 1
    ''')
    
    duplicates = cursor.fetchall()
    if duplicates:
        print(f"Найдены дубликаты: {len(duplicates)}")
        for dup in duplicates:
            print(f"type_id: {dup[0]}, category: {dup[1]}, count: {dup[2]}")
    else:
        print("Дубликаты не обнаружены")
        
    cursor.execute("SELECT COUNT(*) FROM plant_type")
    plant_count = cursor.fetchone()[0]
    cursor.execute("SELECT COUNT(*) FROM flower_type")
    flower_count = cursor.fetchone()[0]
    
    cursor.execute("SELECT COUNT(*) FROM prefill WHERE category='plant'")
    migrated_plants = cursor.fetchone()[0]
    cursor.execute("SELECT COUNT(*) FROM prefill WHERE category='flower'")
    migrated_flowers = cursor.fetchone()[0]
    
    print(f"Original plants: {plant_count}, Migrated: {migrated_plants}")
    print(f"Original flowers: {flower_count}, Migrated: {migrated_flowers}")
    
    cursor.execute('''
    SELECT p.* 
    FROM prefill p
    LEFT JOIN type t ON p.type_id = t.id
    WHERE t.id IS NULL
    ''')
    orphans = cursor.fetchall()
    print(f"Orphaned records: {len(orphans)}")

if __name__ == "__main__":
    main()