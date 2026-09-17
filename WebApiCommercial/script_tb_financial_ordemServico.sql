START TRANSACTION;

ALTER TABLE tb_financial ADD "IdServiceOrder" integer;

CREATE INDEX "IX_tb_financial_IdServiceOrder" ON tb_financial ("IdServiceOrder");

ALTER TABLE tb_financial ADD CONSTRAINT "FK_tb_financial_tb_serviceOrder_IdServiceOrder" FOREIGN KEY ("IdServiceOrder") REFERENCES "tb_serviceOrder" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260917194905_add-financeiro-ordem-servico', '8.0.23');

COMMIT;

