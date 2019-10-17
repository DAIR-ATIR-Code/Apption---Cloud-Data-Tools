FROM microsoft/dotnet:2.1-sdk AS build-env
WORKDIR /App
COPY ./publish  .

EXPOSE 80

WORKDIR /App
ENTRYPOINT ["dotnet", "WebAppMaterialize.Server.dll"]

#To rebuild:
# Publish to file system in directory 'repos\Cloud Data Tools\DataTools\WebAppMaterialize.Server\bin\Debug\netcoreapp2.1\publish'
# Build Docker image in Powershell from directory containing this file using:
#docker build -t webappmaterialize .
# launch from Powershell using:
#docker run -p 8000:80 --rm -it webappmaterialize
# Publish to Azure:
#az login  (Takes you to a browser page)
#az account set --subscription "3b6f5be2-5fcb-4ad7-b202-4b556cb0de98" (if you need to charge to a different subscription)
#az acr login -n ApptionDocker
#docker tag webappmaterialize apptiondocker.azurecr.io/webappmaterialize
#docker push apptiondocker.azurecr.io/webappmaterialize
