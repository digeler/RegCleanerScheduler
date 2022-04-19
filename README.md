# RegCleanerScheduler
Worker Service To Clean Images From Azure Acr

Preq
---------
1. Create the Cosmos Account if it not exist
2. assign permissions: az cosmosdb sql role assignment create --account-name polydgeusegc-scope-database --resource-group  --scope "/" --principal-id [objectid] --role-definition-id 00000000-0000-0000-0000-000000000002 --debug
