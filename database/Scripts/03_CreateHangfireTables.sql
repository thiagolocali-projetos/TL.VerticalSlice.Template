-- Script para criar tabelas do Hangfire
-- O Hangfire criarÃ¡ automaticamente na primeira execuÃ§Ã£o
-- Este script Ã© fornecido como backup para restauraÃ§Ã£o manual

USE TLVerticalSliceTemplate;

GO

-- As tabelas do Hangfire serÃ£o criadas automaticamente pelo Hangfire.SqlServer
-- quando vocÃª chamar app.UseHangfireConfiguration() na primeira vez.
--
-- Se precisar recriar as tabelas manualmente, descomente e execute:
/*
-- Limpar tabelas do Hangfire (CUIDADO - isso deleta todos os jobs!)
DROP TABLE IF EXISTS [HangFire.Job];
DROP TABLE IF EXISTS [HangFire.JobParameter];
DROP TABLE IF EXISTS [HangFire.JobState];
DROP TABLE IF EXISTS [HangFire.State];
DROP TABLE IF EXISTS [HangFire.Counter];
DROP TABLE IF EXISTS [HangFire.AggregatedCounter];
DROP TABLE IF EXISTS [HangFire.DistributedLock];
DROP TABLE IF EXISTS [HangFire.Hash];
DROP TABLE IF EXISTS [HangFire.List];
DROP TABLE IF EXISTS [HangFire.Set];
DROP TABLE IF EXISTS [HangFire.Server];
DROP TABLE IF EXISTS [HangFire.Schedule];

PRINT 'Tabelas do Hangfire removidas. Execute a aplicaÃ§Ã£o para recriÃ¡-las automaticamente.';
*/

PRINT 'As tabelas do Hangfire serÃ£o criadas automaticamente na primeira execuÃ§Ã£o da aplicaÃ§Ã£o.';

