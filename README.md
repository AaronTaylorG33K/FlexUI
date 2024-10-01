 <h1 align="">FlexUI</h1>

FlexUI is a project comprised of Postgres, .NET 8.0 and Remix. It communicates over WebSockets and is setup to run on a scalable Kubernetes cluster and can be centrally controlled using Make commands.



![Screenshot 2024-09-30 at 8 25 27 PM](https://github.com/user-attachments/assets/870c0a66-ee7d-48fe-9e12-c5b19aab51ed)


**Prerequisites**

Docker, Kubernetes, Kubectl, Make
```bash
# Install Docker
brew install --cask docker

# Install Kubernetes CLI (kubectl)
brew install kubectl

# Install Kubernetes (Minikube)
brew install minikube

# Install make
brew install make
```


**Main Commands**

Start the stack with fresh.

```bash
make init
```

Kill the entire stack. 

```bash
make kill
```

Dev mode - start only Postgres
```bash
make init-local
```
<br/>
<h2>Make Commands</h2>
<p>The following are a list of commands that allow you to granularly control the application in it's kubernetes cluster.</p>
<h3>Build Commands</h3>
<ul>
  <li><strong>build</strong>: Build all Docker images.
    <pre><code>make build</code></pre>
  </li>
  <li><strong>build-dotnet</strong>: Build the .NET Docker image.
    <pre><code>make build-dotnet</code></pre>
  </li>
  <li><strong>build-remix</strong>: Build the Remix Docker image.
    <pre><code>make build-remix</code></pre>
  </li>
  <li><strong>build-postgres</strong>: Build the PostgreSQL Docker image.
    <pre><code>make build-postgres</code></pre>
  </li>
</ul>

<h3>Registry Commands</h3>
<ul>
  <li><strong>ensure-registry</strong>: Ensure the local Docker registry is running.
    <pre><code>make ensure-registry</code></pre>
  </li>
</ul>

<h3>Namespace Commands</h3>
<ul>
  <li><strong>create-namespace</strong>: Create the Kubernetes namespace if it doesn't exist.
    <pre><code>make create-namespace</code></pre>
  </li>
</ul>

<h3>Push Commands</h3>
<ul>
  <li><strong>push</strong>: Tag and push all Docker images to the registry.
    <pre><code>make push</code></pre>
  </li>
</ul>

<h3>Deploy Commands</h3>
<ul>
  <li><strong>deploy</strong>: Deploy all services to Kubernetes.
    <pre><code>make deploy</code></pre>
  </li>
  <li><strong>deploy-postgres</strong>: Deploy only PostgreSQL to Kubernetes.
    <pre><code>make deploy-postgres</code></pre>
  </li>
</ul>

<h3>Port Forwarding Commands</h3>
<ul>
  <li><strong>port-forward</strong>: Port forward all services.
    <pre><code>make port-forward</code></pre>
  </li>
  <li><strong>port-forward-postgres</strong>: Port forward PostgreSQL service.
    <pre><code>make port-forward-postgres</code></pre>
  </li>
</ul>

<h3>Log Commands</h3>
<ul>
  <li><strong>tail-logs</strong>: Tail logs for all pods.
    <pre><code>make tail-logs</code></pre>
  </li>
</ul>

<h3>Stop Commands</h3>
<ul>
  <li><strong>stop</strong>: Scale down all deployments and stop port forwarding.
    <pre><code>make stop</code></pre>
  </li>
</ul>

<h3>Clean Commands</h3>
<ul>
  <li><strong>clean-pods</strong>: Delete all Kubernetes pods.
    <pre><code>make clean-pods</code></pre>
  </li>
  <li><strong>clean-services</strong>: Delete all Kubernetes services.
    <pre><code>make clean-services</code></pre>
  </li>
  <li><strong>clean-images</strong>: Remove local Docker images.
    <pre><code>make clean-images</code></pre>
  </li>
  <li><strong>clean-ports</strong>: Clean up port forwarding.
    <pre><code>make clean-ports</code></pre>
  </li>
  <li><strong>clean</strong>: Stop and clean the entire stack.
    <pre><code>make clean</code></pre>
  </li>
</ul>

<h3>Initialization Commands</h3>
<ul>
  <li><strong>init</strong>: First run command to build, push, deploy, initialize the database, and port forward.
    <pre><code>make init</code></pre>
  </li>
  <li><strong>init-local</strong>: Initialize PostgreSQL in Kubernetes and use local environment for .NET and Remix.
    <pre><code>make init-local</code></pre>
  </li>
</ul>

<h3>Database Commands</h3>
<ul>
  <li><strong>run-schema</strong>: Run the schema SQL script.
    <pre><code>make run-schema</code></pre>
  </li>
  <li><strong>run-seed</strong>: Run the seed SQL script.
    <pre><code>make run-seed</code></pre>
  </li>
  <li><strong>init-db</strong>: Initialize the database by running schema and seed scripts.
    <pre><code>make init-db</code></pre>
  </li>
</ul>

<h3>Miscellaneous Commands</h3>
<ul>
  <li><strong>start</strong>: Start the entire stack.
    <pre><code>make start</code></pre>
  </li>
  <li><strong>kill</strong>: Delete the Kubernetes namespace.
    <pre><code>make kill</code></pre>
  </li>
</ul>
