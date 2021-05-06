#dotnet commands

dotnet publish -o ./publish

# docker commands build
docker build -t testapp . #include the . # <> is the projectname
docker run -p 80:80 testapp:latest


