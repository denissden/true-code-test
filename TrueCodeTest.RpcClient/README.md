# RpcClient

## Overview

## Features

- **Node discovery:** Client has ability to request available nodes and their supported procedures.
- **RPC:** Remote procedure calls.
    - **Shared:** Call procedure on any first-available node.
    - **Nodelet:** Call procedure on a specific nodelet.
- **Cancellation:** Cancel long-running procedures.

## Concepts

**Hub** is responsible for connecting to RabbitMQ.

- Hub has _one connection factory_.
- Hub may have _multiple connections_.
- Hub may have _multiple nodes_.

**Node** is responsible for creating channels and nodeletes.

- Node has _one connection_.
- Node creates channels and passes them to Nodelets.
- Node may create _one or more channels_.

**Nodelet** is responsible for dispatching incoming messages to handlers.

- Nodelet has a _unique id_.
- Nodelet has _one channel_.
- Nodelet dispatches requests from _zero or more topics_.
- Nodelet listens on _multiple queues_.

## Design decisions

## Cancellation queue

The main requirement of the system is to handle RPC cancellation properly.
The first idea that may come to mind is using the same queue for requests and cancellation requests.
What happens when the queue starts filling up beyond PrefetchCount? The cancellation message will never reach the target
in time.
Using priority queues also doesn't always help - messages will be moved to the end of the queue, but still get stuck if
PrefetchCount doesn't allow them to be consumed. It was decided to use a different queue for cancellation requests -
this way the only bottleneck is the consumer
itself. [Read more about PrefetchCount](https://www.rabbitmq.com/docs/consumer-prefetch)

## Discovery queue

For the same reasons as cancellation queue, discovery queue is also separated. First-responded-nodelet discoveries
should happen fast, without even waiting for cancellation requests. It is also quite nifty to separate cancellation and
discovery logic into different handlers to avoid potential errors and make the system more error-resistant.