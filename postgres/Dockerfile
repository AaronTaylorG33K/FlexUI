# Use the official PostgreSQL image as a base
FROM postgres:latest

# Environment variables for user, password, and database
ENV POSTGRES_USER=myuser
ENV POSTGRES_PASSWORD=mypassword
ENV POSTGRES_DB=mydb

COPY ./sql/schema.sql /docker-entrypoint-initdb.d/
COPY ./sql/seeds.sql /docker-entrypoint-initdb.d/

# Expose the PostgreSQL port
EXPOSE 5432
