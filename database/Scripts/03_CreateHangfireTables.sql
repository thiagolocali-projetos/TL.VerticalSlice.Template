-- Script para criar tabelas do Hangfire
-- O Hangfire criará automaticamente na primeira execução
-- Este script é fornecido como backup para restauração manual

USE TLExemplo;

GO

-- As tabelas do Hangfire serão criadas automaticamente pelo Hangfire.SqlServer
-- quando você chamar app.UseHangfireConfiguration() na primeira vez.
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

PRINT 'Tabelas do Hangfire removidas. Execute a aplicação para recriá-las automaticamente.';
*/

PRINT 'As tabelas do Hangfire serão criadas automaticamente na primeira execução da aplicação.';
