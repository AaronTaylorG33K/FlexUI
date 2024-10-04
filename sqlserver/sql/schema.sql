-- Drop the 'page_components' table first because it has foreign key references to 'pages' and 'components'
DROP TABLE IF EXISTS page_components;

-- Drop the 'components' table
DROP TABLE IF EXISTS components;

-- Drop the 'pages' table
DROP TABLE IF EXISTS pages;

-- Recreate the 'pages' table
CREATE TABLE pages (
    id SERIAL PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    content TEXT NOT NULL
);

-- Recreate the 'components' table
CREATE TABLE components (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    settings JSONB
);

-- Create the 'page_components' table to represent the many-to-many relationship
CREATE TABLE page_components (
    id SERIAL PRIMARY KEY,
    page_id INT NOT NULL,
    component_id INT NOT NULL,
    ordinal INT NOT NULL,
    FOREIGN KEY (page_id) REFERENCES pages(id) ON DELETE CASCADE,
    FOREIGN KEY (component_id) REFERENCES components(id) ON DELETE CASCADE
);