from packaging import version
import requests
import json
import os
import sys
import zipfile
import win32api


class updater:
    def __init__(self):
        with open('../info/version.json', 'rt') as f:
            self.cur = version.parse(json.load(f)['version'])

        res = requests.get(
            'https://mgylabs.gitlab.io/discord-bot/version.json')
        data = res.json()
        self.last = version.parse(data['last-version'])
        self.tags = data['tags']

    def check_update(self):
        return self.last > self.cur

    def isnewversion(self):
        if self.check_update():
            sys.exit(0)
        else:
            sys.exit(1)

    def run(self):
        if self.check_update():
            win32api.ShellExecute(None, "open", "taskkill",
                                  '/f /im "MKBot.exe"', None, 0)
            r = requests.get(
                'https://gitlab.com/mgylabs/discord-bot/-/jobs/artifacts/{}/download?job=stable-release'.format(self.tags))

            download_file_name = os.getenv(
                'USERPROFILE')+'\\Downloads\\MKBotSetup.zip'

            with open(download_file_name, 'wb') as f:
                f.write(r.content)

            _zip = zipfile.ZipFile(download_file_name)
            _zip.extractall(os.getenv('USERPROFILE')+'\\Downloads')

            win32api.ShellExecute(None, "open", os.getenv(
                'USERPROFILE')+'\\Downloads\\MKBotSetup.exe', "/S", None, 0)
        else:
            sys.exit(1)


ut = updater()
if '/c' in sys.argv:
    ut.isnewversion()
else:
    ut.run()
