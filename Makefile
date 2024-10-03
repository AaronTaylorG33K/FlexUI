IMAGE_NAME_DOTNET = dotnet
IMAGE_NAME_REMIX = remix
IMAGE_NAME_POSTGRES = postgres
TAG = latest
REGISTRY = localhost:6000
NAMESPACE = flexui

# Define paths to Dockerfiles
DOTNET_DOCKERFILE = dotnet/Dockerfile
REMIX_DOCKERFILE = remix/Dockerfile
POSTGRES_DOCKERFILE = postgres/Dockerfile

# Define path to Kubernetes deployments and services files
DEPLOYMENTS_FILE = deployments.yaml
SERVICES_FILE = services.yaml

# Define port forwarding ports
PORT_DOTNET = 8080
PORT_POSTGRES = 8081
PORT_REMIX = 3000

# Define paths to SQL files
SCHEMA_FILE := ./postgres/scripts/schema.sql
SEED_FILE := ./postgres/scripts/seeds.sql

# Define PostgreSQL pod name
POSTGRES_POD_NAME := $(shell kubectl get pods -n $(NAMESPACE) -l app=postgres -o jsonpath="{.items[0].metadata.name}")

# Build all images
build: build-dotnet build-remix build-postgres
	@echo "🔥 Images ready to go."

# Ensure local Docker registry is running
ensure-registry:
	@if [ -z "$$(docker ps -q -f name=registry)" ]; then \
    	echo "Starting local Docker registry..."; \
    	docker run -d -p 6000:5000 --name registry registry:2; \
	fi

# Create the namespace if it doesn't exist
create-namespace:
	@if ! kubectl get namespace $(NAMESPACE) > /dev/null 2>&1; then \
    	echo "Creating namespace $(NAMESPACE)..."; \
    	kubectl create namespace $(NAMESPACE); \
	fi

# Build the .NET Docker image
build-dotnet:
	@echo "Building .NET Docker image..."
	docker build -t $(IMAGE_NAME_DOTNET):$(TAG) -f $(DOTNET_DOCKERFILE) dotnet

# Build the React Docker image
build-remix:
	@echo "Building Remix Docker image..."
	docker build -t $(IMAGE_NAME_REMIX):$(TAG) -f $(REMIX_DOCKERFILE) remix

# Build the PostgreSQL Docker image
build-postgres:
	@echo "Building PostgreSQL Docker image..."
	docker build -t $(IMAGE_NAME_POSTGRES):$(TAG) -f $(POSTGRES_DOCKERFILE) postgres

# Tag and push all Docker images
push: ensure-registry create-namespace
	@echo "Waiting for registry to be fully up..."
	@echo "Tagging and pushing Docker images..."
	docker tag $(IMAGE_NAME_DOTNET):$(TAG) $(REGISTRY)/$(IMAGE_NAME_DOTNET):$(TAG)
	docker tag $(IMAGE_NAME_REMIX):$(TAG) $(REGISTRY)/$(IMAGE_NAME_REMIX):$(TAG)
	docker tag $(IMAGE_NAME_POSTGRES):$(TAG) $(REGISTRY)/$(IMAGE_NAME_POSTGRES):$(TAG)
	docker push $(REGISTRY)/$(IMAGE_NAME_DOTNET):$(TAG)
	docker push $(REGISTRY)/$(IMAGE_NAME_REMIX):$(TAG)
	docker push $(REGISTRY)/$(IMAGE_NAME_POSTGRES):$(TAG)

# Tag and push all Docker images
push-postgres: ensure-registry create-namespace
	@echo "Waiting for registry to be fully up..."
	@echo "Tagging and pushing Docker images..."
	docker tag $(IMAGE_NAME_POSTGRES):$(TAG) $(REGISTRY)/$(IMAGE_NAME_POSTGRES):$(TAG)
	docker push $(REGISTRY)/$(IMAGE_NAME_POSTGRES):$(TAG)

# Deploy to Kubernetes
deploy:
	@echo "Applying Kubernetes deployments..."
	kubectl apply -f $(DEPLOYMENTS_FILE) -n $(NAMESPACE)
	@echo "Applying Kubernetes services..."
	kubectl apply -f $(SERVICES_FILE) -n $(NAMESPACE)
	@echo "Waiting for pods to be ready..."
    kubectl wait --for=condition=available --timeout=60s deployment/dotnet -n $(NAMESPACE)
    kubectl wait --for=condition=available --timeout=60s deployment/remix -n $(NAMESPACE)
    kubectl wait --for=condition=available --timeout=60s deployment/postgres -n $(NAMESPACE)

# # Port forward all services
port-forward:
	@echo "Namespace: $(NAMESPACE)"
	@echo "Port forwarding .NET..."
	kubectl port-forward service/dotnet $(PORT_DOTNET):80 -n $(NAMESPACE) &
	@echo "Port forwarding Remix..."
	kubectl port-forward service/remix $(PORT_REMIX):3000 -n $(NAMESPACE) &

# Tail logs for all pods
tail-logs:
	@echo "Tailing logs for .NET pods..."
	kubectl logs -f -l app=dotnet -n $(NAMESPACE)
	@echo "Tailing logs for PostgreSQL pods..."
	kubectl logs -f -l app=postgres -n $(NAMESPACE)
	@echo "Tailing logs for Remix pods..."
	kubectl logs -f -l app=remix -n $(NAMESPACE)

# Scale down all deployments and delete all pods
stop:
	@echo "Scaling down deployments..."
	kubectl scale deployment dotnet --replicas=0 -n $(NAMESPACE)
	kubectl scale deployment remix --replicas=0 -n $(NAMESPACE)
	kubectl scale deployment postgres --replicas=0 -n $(NAMESPACE)
	@echo "Stopping port forwarding..."
	pkill -f "kubectl port-forward"
	@echo "Stack stopped."

# Clean up Kubernetes pods
clean-pods:
	@echo "Deleting all pods..."
	kubectl delete pods --all -n $(NAMESPACE)
	@echo "Pods deleted."

# Clean up Kubernetes services
clean-services:
	@echo "Deleting all services..."
	kubectl delete services --all -n $(NAMESPACE)
	@echo "Services deleted."

# Clean up local Docker images
clean-images:
	@echo "Removing local Docker images..."
	docker rmi $(IMAGE_NAME_DOTNET):$(TAG) $(IMAGE_NAME_REMIX):$(TAG) $(IMAGE_NAME_POSTGRES):$(TAG)
# Clean up port forwarding
clean-ports:
	@echo "Cleaning up port forwarding..."
	pkill -f "kubectl port-forward"
# Command to start the entire stack
start: push deploy port-forward
	@echo "Opening http://localhost:3000 in the browser..."
	open http://localhost:3000; 
	@echo "Opening http://localhost:8080/swagger in the browser..."
	open http://localhost:8080/swagger;
	@echo "💪 FlexUI started. Lets goooo!!!"
	@echo ""

# Command to stop and clean the stack
clean:
	@echo "🔥\033[0;31mK\033[0;33mi\033[0;31ml\033[0;33ml\033[0;31m \033[0;33mi\033[0;31mt\033[0;33m \033[0;31mw\033[0;33mi\033[0;31mt\033[0;33mh\033[0;31m \033[0;33mf\033[0;31mi\033[0;33mr\033[0;31me\033[0;33m!\033[0m"
	$(MAKE) clean-pods
	$(MAKE) clean-services
	$(MAKE) clean-images
	$(MAKE) stop
#lolz
kill:
	kubectl delete namespace $(NAMESPACE)
	@echo "💀💀💀💀💀"

# First run command
init: build push deploy init-db port-forward
	@echo "Opening http://localhost:3000 in the browser..."
	@if ! pgrep -f "open http://localhost:3000" > /dev/null; then \
		open http://localhost:3000; \
	fi
	@echo "💪 FlexUI started. Lets goooo!!!"
	@echo "Run \033[0;32mmake tail-logs\033[0m to view live logs"

#port-forward-postgres
init-local: ensure-registry create-namespace build-postgres push-postgres deploy-postgres init-db port-forward-postgres
	@echo "💪 PostgreSQL initialized in Kubernetes. Use your local environment for .NET and Remix."

	

# Deploy only PostgreSQL to Kubernetes
deploy-postgres:
	@echo "Applying PostgreSQL deployment..."
	kubectl apply -f $(DEPLOYMENTS_FILE) -n $(NAMESPACE) -l app=postgres
	@echo "Applying PostgreSQL service..."
	kubectl apply -f $(SERVICES_FILE) -n $(NAMESPACE) -l app=postgres
	@echo "Waiting for PostgreSQL pod to be ready 2..."
    kubectl wait --for=condition=available --timeout=60s deployment/postgres -n $(NAMESPACE)

# Port forward PostgreSQL service
port-forward-postgres:
	@echo "Port forwarding PostgreSQL..."
	kubectl port-forward service/postgres $(PORT_POSTGRES):5432 -n $(NAMESPACE) &


# Wait for PostgreSQL pod to be ready
wait-for-postgres:
	@echo "wait-for-postgres: Waiting for PostgreSQL pod to be ready..."
	@kubectl wait --for=condition=Ready pod -l app=postgres -n $(NAMESPACE) --timeout=120s
	@sleep 5



# Run schema SQL script
run-schema: wait-for-postgres
	@echo "Running schema SQL script..."
	@POSTGRES_POD_NAME=$$(kubectl get pods -n $(NAMESPACE) -l app=postgres -o jsonpath="{.items[0].metadata.name}") && \
	kubectl exec -i $$POSTGRES_POD_NAME -n $(NAMESPACE) -- psql -U myuser -d mydb -f /docker-entrypoint-initdb.d/schema.sql

# Run seed SQL script
run-seed: wait-for-postgres
	@echo "Running seed SQL script..."
	@POSTGRES_POD_NAME=$$(kubectl get pods -n $(NAMESPACE) -l app=postgres -o jsonpath="{.items[0].metadata.name}") && \
	kubectl exec -i $$POSTGRES_POD_NAME -n $(NAMESPACE) -- psql -U myuser -d mydb -f /docker-entrypoint-initdb.d/seeds.sql

# Initialize the database
init-db: run-schema run-seed
	@echo "\033[0;33m🔥 Databases locked and loaded\033[0m";
	@echo "➡️ Now run \033[0;32m make start\033[0m";