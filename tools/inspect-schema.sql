-- Перевірка структури БД на Render: psql "<External Database URL>" -f tools/inspect-schema.sql
\echo '== Таблиці =='
\dt
\echo '== Servers =='
\d "Servers"
\echo '== MetricSamples =='
\d "MetricSamples"
\echo '== Застосовані міграції EF Core =='
SELECT "MigrationId", "ProductVersion" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
\echo '== Кількість записів =='
SELECT (SELECT count(*) FROM "Servers") AS servers, (SELECT count(*) FROM "MetricSamples") AS metrics;
