```mermaid
flowchart LR
    subgraph Application
        Interface["Application(interface)"]
        Services["Application(service)"]
        Services --> Interface
    end
    Api --> Services
    Api -.Register DI.-> Infrastructure
    Application --> Domain
    Infrastructure --> Domain
    Infrastructure --> Interface
    Infrastructure ---- DB
    subgraph Storage
        DB
    end
```