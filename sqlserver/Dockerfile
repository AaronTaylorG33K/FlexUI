# Use the official Microsoft SQL Server image as a base
FROM mcr.microsoft.com/mssql/server:latest

# Environment variables for user, password, and database
ENV SA_PASSWORD=YourStrong!Passw0rd
ENV ACCEPT_EULA=Y

# Copy the SQL scripts to the appropriate directory
COPY ./sql/schema.sql /var/opt/mssql/data/
COPY ./sql/seeds.sql /var/opt/mssql/data/

# Expose the SQL Server port
EXPOSE 1433

# Run the SQL Server and initialize the database
CMD /opt/mssql/bin/sqlservr & sleep 30 && \
    /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P YourStrong!Passw0rd -d master -i /var/opt/mssql/data/schema.sql && \
    /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P YourStrong!Passw0rd -d master -i /var/opt/mssql/data/seeds.sql && \
    wait