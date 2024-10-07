using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Threading.Tasks;
using FlexUI.Models;
using Microsoft.Extensions.Configuration;

namespace FlexUI.Services
{
    public class FlexUI
    {
        private readonly string _connectionString;

        public FlexUI(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
        }

        public async Task<object> GetAllData()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(@"
                    SELECT 
                        (SELECT 
                            JSON_QUERY(
                                (SELECT 
                                    JSON_QUERY(
                                        (SELECT 
                                            p.id AS id,
                                            p.title AS title,
                                            p.slug AS slug,
                                            p.content AS content,
                                            (SELECT 
                                                JSON_QUERY(
                                                    (SELECT 
                                                        c.id AS component_id,
                                                        pc.id AS id,
                                                        c.name AS name,
                                                        c.settings AS settings,
                                                        pc.ordinal AS ordinal
                                                    FOR JSON PATH)
                                                ) AS components
                                            FROM page_components pc
                                            JOIN components c ON pc.component_id = c.id
                                            WHERE pc.page_id = p.id
                                            FOR JSON PATH
                                        )
                                    FOR JSON PATH)
                                )
                            ) AS pages
                        FROM pages p
                        FOR JSON PATH
                    ) AS data;
                ", connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    return JsonSerializer.Deserialize<object>(result?.ToString() ?? string.Empty);
                }
            }
        }

        public async Task ProcessMutationsAsync(List<Mutation> mutations)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    foreach (var mutation in mutations)
                    {
                        using (var command = new SqlCommand())
                        {
                            command.Connection = connection;
                            command.Transaction = (SqlTransaction)transaction;

                            if (mutation.Type == "ordinalUpdate")
                            {
                                command.CommandText = @"
                                    UPDATE page_components
                                    SET 
                                        ordinal = @newOrdinal
                                    WHERE id = @pageComponentID;
                                ";
                                command.Parameters.AddWithValue("@newOrdinal", mutation.NewOrdinal);
                                command.Parameters.AddWithValue("@pageComponentID", mutation.PageComponentID);

                                Console.WriteLine($"Executing ordinal update: UPDATE page_components SET ordinal = {mutation.NewOrdinal} WHERE id = {mutation.PageComponentID} - {mutation.ComponentName}");
                            }
                            else if (mutation.Type == "pageMove")
                            {
                                command.CommandText = @"
                                    UPDATE page_components
                                    SET 
                                        page_id = @destinationPageID
                                    WHERE id = @pageComponentID;
                                ";
                                command.Parameters.AddWithValue("@destinationPageID", mutation.DestinationPageID);
                                command.Parameters.AddWithValue("@pageComponentID", mutation.PageComponentID);

                                Console.WriteLine($"Executing page move: UPDATE page_components SET page_id = {mutation.DestinationPageID} WHERE id = {mutation.PageComponentID} - {mutation.ComponentName}");
                            }
                            else
                            {
                                Console.WriteLine($"Unknown mutation type: {mutation.Type}");
                                continue; // Skip unknown mutation types
                            }

                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    await transaction.CommitAsync();
                }
            }
        }
    }
}