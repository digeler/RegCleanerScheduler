# RegCleanerScheduler
Worker Service To Clean Images From Azure Acr

Operation 
---------------
This is a background service the composed of two parts ,main drive is to purge old images from container registry
1. Dump all the container registry contents to CosmosDb.
2. Purge All Images According to this query : 
SELECT * FROM c where c.LastUpdated < DateTimeAdd("month", [MonthsBack], GetCurrentDateTime()) order by c.LastUpdated desc

Preq
---------
1. Cluster must have podidentity installed
2. Create the Cosmos Account if it not exists
3. On Each env only one instance is needed
4. Deploy the yaml in the deployment folder or package to helm file.

Dockerfile
------------------
....RegCleanerScheduler\Dockerfile

Params
----------
apiVersion: apps/v1
kind: Deployment
metadata:
  name: regcleaner
  labels:
    app: regcleaner
spec:
  replicas: 1
  selector:
    matchLabels:
      app: regcleaner
  template:
    metadata:
      labels:
        app: regcleaner
        aadpodidbinding: [Your Identity Selector]
    spec:
      containers:
      - name: regcleaner
        image: [Image Name]     
        env:
          - name: "RegEndpoint"
            value: "Registry Endpoint e.g [https://someregistry.azurecr.io]"
          - name: "MonthsBack"
            value: "Month Images To Be Removed ,e.g 3 years of old images [-36]"
          - name: "CosmosDbName"
            value: "Cosmos Db Name - Will Create If Not Exist"
          - name: "CosmosDbAccountName"
            value: "Cosmos Db Account - You Will Need To Create It Or Use Existing One"
          - name: "CosmosContainerName"
            value: "Container Name - Will Be Created If Not Exist"
          - name: "SubscriptionId"
            value: "Subid"
          - name: "ResourceGroup"
            value: "The Resource Group Where Your Cosmos Account Is Defined"
          - name: IsDryRun
            value: "The Operation Mode Of Image Deletion e.g[True] ,Will Not Delete Images -Default is True "
          - name: "ScheduleDumpCron"
            value: "The Time You Want To Run The Dump To Cosmos Operation e.g [0 23 * * 0] Default Every Sunday At 23:00"
          - name:  "ScheduleDeleteImages"
            value: "The Time You Want To Executre The Delete Operaion e.g [0 23 * * 1] Default Every Monday At 23:00"
           

         



         
          
       


