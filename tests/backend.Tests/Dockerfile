#source: https://github.com/nats-io/nats-docker/blob/060868360489ec6400f3238d633b986103445dd5/2.2.0/scratch/Dockerfile

FROM scratch
COPY --from=nats:2.2.0-alpine3.13 /usr/local/bin/nats-server /nats-server
COPY nats-serverA.conf /nats-serverA.conf
EXPOSE 4222 8222 6222
ENTRYPOINT ["/nats-server"]
CMD ["--config", "nats-serverA.conf"]
