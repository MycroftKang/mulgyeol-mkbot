"""add update_at and create_at to hashstore

Revision ID: ab4b02ba328c
Revises: fcdada5cf898
Create Date: 2023-01-11 11:49:59.268747

"""
from alembic import op
import sqlalchemy as sa


# revision identifiers, used by Alembic.
revision = "ab4b02ba328c"
down_revision = "fcdada5cf898"
branch_labels = None
depends_on = None


def upgrade():
    # ### commands auto generated by Alembic - please adjust! ###
    op.add_column("hash_store", sa.Column("update_at", sa.DateTime(), nullable=True))
    op.add_column("hash_store", sa.Column("created_at", sa.DateTime(), nullable=True))
    # ### end Alembic commands ###


def downgrade():
    # ### commands auto generated by Alembic - please adjust! ###
    op.drop_column("hash_store", "created_at")
    op.drop_column("hash_store", "update_at")
    # ### end Alembic commands ###
