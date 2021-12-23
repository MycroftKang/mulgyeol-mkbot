"""Add Hash Store

Revision ID: 73bce8369050
Revises: d7cee7a58811
Create Date: 2021-12-23 17:53:58.389985

"""
from alembic import op
import sqlalchemy as sa


# revision identifiers, used by Alembic.
revision = "73bce8369050"
down_revision = "d7cee7a58811"
branch_labels = None
depends_on = None


def upgrade():
    # ### commands auto generated by Alembic - please adjust! ###
    op.create_table(
        "hash_store",
        sa.Column("key", sa.String(), nullable=False),
        sa.Column("value", sa.String(), nullable=True),
        sa.PrimaryKeyConstraint("key"),
    )
    # ### end Alembic commands ###


def downgrade():
    # ### commands auto generated by Alembic - please adjust! ###
    op.drop_table("hash_store")
    # ### end Alembic commands ###
