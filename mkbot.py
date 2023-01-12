import os
import sys

os.chdir("src/bot")
sys.path.append("../lib")

from mgylabs.db.database import run_migrations
from mgylabs.db.paths import DB_URL, SCRIPT_DIR

run_migrations(SCRIPT_DIR, DB_URL, echo=True)
