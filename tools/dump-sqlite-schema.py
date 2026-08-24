import sqlite3
import sys

path = sys.argv[1]
con = sqlite3.connect(path)
print("---tables---")
for (name,) in con.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name"):
    print(name)
print("---schema---")
for (sql,) in con.execute("SELECT sql FROM sqlite_master WHERE type='table' ORDER BY name"):
    print(sql)
    print()
