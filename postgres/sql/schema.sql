-- Drop the 'components' table first because it has a foreign key reference to 'pages'
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

-- Recreate the 'components' table with a foreign key reference to 'pages'
CREATE TABLE components (
    id SERIAL PRIMARY KEY,
    page_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    settings JSONB,
    ordinal INT NOT NULL,
    FOREIGN KEY (page_id) REFERENCES pages(id) ON DELETE CASCADE
);