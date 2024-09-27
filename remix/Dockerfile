# Stage 1: Build the application
FROM node:18-alpine AS builder

# Set the working directory in the container
WORKDIR /app

# Copy the package.json and package-lock.json or yarn.lock
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the app source code to the working directory
COPY . .

# Build the Remix app
RUN npm run build

# Stage 2: Serve the application
FROM node:18-alpine

# Set the working directory in the container
WORKDIR /app

# Copy over the `node_modules` and build output from the builder stage
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/build ./build
COPY --from=builder /app/public ./public
COPY --from=builder /app/package*.json ./

# Expose the port Remix will run on
EXPOSE 3000

# Start the Remix app
CMD ["npm", "run", "start"]
