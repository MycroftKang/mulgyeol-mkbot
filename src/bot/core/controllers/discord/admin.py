import discord
from discord.ext import commands

from mgylabs.i18n import _
from mgylabs.utils import logger
from mgylabs.utils.config import CONFIG
from mgylabs.utils.nlu import NluModel

from .utils.MGCert import Level, MGCertificate

log = logger.get_logger(__name__)


class Admin(commands.Cog):
    def __init__(self, bot):
        self.bot: commands.Bot = bot
        self.install_working = set()
        self.load_working = set()

    @commands.command(hidden=True)
    @MGCertificate.verify(level=Level.ADMIN_USERS)
    async def install(self, ctx: commands.Context, *, query):
        if query in self.install_working:
            return

        self.install_working.add(query)

        if query == "nlu_ko":
            msg: discord.Message = await ctx.send(
                "<a:typing:1073250215974420490> " + _("Installing... `%s`") % query
            )

            from mkbot_nlu.nlu import MKBotNLU

            log.info(f"Downloading {query}...")
            await MKBotNLU.download_ko_wiki_model()

            log.info(f"Loading {query}...")
            NluModel.load()

            CONFIG.enabledChatMode = True

            log.info(f"Successfully installed {query}...")
            await msg.edit(content=_("🟢 Successfully installed `%s`") % query)
        else:
            await ctx.send("Invalid!")

        self.install_working.remove(query)

    @commands.command(hidden=True)
    @MGCertificate.verify(level=Level.ADMIN_USERS)
    async def load(self, ctx: commands.Context, *, query):
        if query in self.load_working:
            return

        self.load_working.add(query)

        if query == "nlu_ko":
            msg: discord.Message = await ctx.send(
                "<a:typing:1073250215974420490> " + _("Loading... `%s`") % query
            )

            log.info(f"Loading {query}...")
            NluModel.load()

            CONFIG.enabledChatMode = True

            log.info(f"Successfully loaded {query}...")
            await msg.edit(content=_("🟢 Successfully loaded `%s`") % query)
        else:
            await ctx.send("Invalid!")

        self.load_working.remove(query)


async def setup(bot: commands.Bot):
    await bot.add_cog(Admin(bot))
