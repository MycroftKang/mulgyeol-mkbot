import datetime
import platform

import discord
from discord.ext import commands
from mgylabs.utils.config import CONFIG, VERSION

Msg_Color = None


def get_color():
    global Msg_Color
    if Msg_Color == None:
        c = CONFIG.messageColor
        c = c.replace("#", "")
        Msg_Color = int(c, 16)
        return Msg_Color
    else:
        return Msg_Color


def color_to_int(code):
    return int(code.replace("#", ""), 16)


class MsgFormatter:
    avatar_url: str = None

    @staticmethod
    def set_avatar_url(avatar_url):
        MsgFormatter.avatar_url = avatar_url

    @staticmethod
    def get(
        ctx,
        title,
        description="",
        fields: list = [],
        show_req_user=True,
        *,
        color: str = None,
    ):
        if color is None:
            color = get_color()
        else:
            color = color_to_int(color)

        embed = discord.Embed(
            title=title,
            description=description.format(commandPrefix=CONFIG.commandPrefix)
            + "\n\nPowered by [Mulgyeol MK Bot](https://github.com/mgylabs/mulgyeol-mkbot)",
            color=color,
            timestamp=datetime.datetime.utcnow(),
        )

        for fd in fields:
            embed.add_field(**fd)

        if show_req_user:
            if isinstance(ctx, commands.Context):
                user_id = ctx.author.id
            elif isinstance(ctx, discord.Interaction):
                user_id = ctx.user.id

            embed.add_field(name="Requested by", value="<@{}>".format(user_id))
        embed.set_footer(text="© Mulgyeol Labs 2022", icon_url=MsgFormatter.avatar_url)
        return embed

    @staticmethod
    def push(title, description="", fields: list = []):
        embed = discord.Embed(
            title=title,
            description=description,
            color=get_color(),
            timestamp=datetime.datetime.utcnow(),
        )

        for fd in fields:
            embed.add_field(**fd)

        embed.set_footer(text="© Mulgyeol Labs 2022", icon_url=MsgFormatter.avatar_url)
        return embed

    @staticmethod
    def abrt(ctx, issue_link, tb, show_req_user=True):
        description = f"Please [create an issue]({issue_link}) at GitHub with logs below to help fix this problem."
        env = f"Version: {VERSION}\nCommit: {VERSION.commit}\nOS: {platform.platform().replace('-', ' ')}"

        embed = discord.Embed(
            title="ABRT: An unknown error has occurred :face_with_monocle:",
            description=f"{description}\n\n```{tb}\n\n{env}```\nPowered by [Mulgyeol MK Bot](https://github.com/mgylabs/mulgyeol-mkbot)",
            color=color_to_int("#FF0000"),
            timestamp=datetime.datetime.utcnow(),
        )

        if show_req_user:
            embed.add_field(name="Requested by", value="<@{}>".format(ctx.author.id))
        embed.set_footer(text="© Mulgyeol Labs 2022", icon_url=MsgFormatter.avatar_url)
        return embed
