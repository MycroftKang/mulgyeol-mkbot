from MGBotBuilder import CommandConsole

from core.controllers.shell.music_controller import music_controller

shell = CommandConsole()
shell.extend(music_controller)