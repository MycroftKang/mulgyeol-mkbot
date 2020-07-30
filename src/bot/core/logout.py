from discord.ext import commands


def setup(bot):
    @commands.command()
    @bot.MGCert.verify(2)
    async def logout(ctx):
        """
        봇을 종료합니다. (관리자 권한 필요)
        """
        await ctx.send(embed=bot.replyformat.get(ctx, "Logs out of Discord and closes all connections"))
        print('Logged out')
        await bot.logout()

    bot.add_command(logout)
