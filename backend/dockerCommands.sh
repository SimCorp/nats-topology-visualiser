#dotnet commands

dotnet publish -o ./publish

# docker commands build
docker build -t testapp . #include the . # <> is the projectname

docker run -p 80:80 frontend
docker run -p 81:80 -e NATS_URL=nats://<username>:<password>@<host>:4222/ backend