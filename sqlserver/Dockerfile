# Use the official Microsoft SQL Server image as a base
FROM mcr.microsoft.com/mssql/server:latest

# Environment variables for user, password, and database
ENV SA_PASSWORD=YourStrong!Passw0rd
ENV ACCEPT_EULA=Y

# Copy the SQL scripts to the appropriate directory
COPY ./sql/schema.sql /var/opt/mssql/data/
COPY ./sql/seeds.sql /var/opt/mssql/data/

# Set the correct permissions for the mssql user
USER root
RUN chown -R mssql:mssql /var/opt/mssql

# Switch back to the mssql user
USER mssql

# Expose the SQL Server port
EXPOSE 1433


# Run the SQL Server
CMD /opt/mssql/bin/sqlservr