-- Drop the 'page_components' table first because it has foreign key references to 'pages' and 'components'
DROP TABLE IF EXISTS page_components;

-- Drop the 'components' table
DROP TABLE IF EXISTS components;

-- Drop the 'pages' table
DROP TABLE IF EXISTS pages;
-- Create 'pages' table
CREATE TABLE pages (
    id INT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(255) NOT NULL,
    slug NVARCHAR(255) NOT NULL,
    content NVARCHAR(MAX) NOT NULL
);

-- Create 'components' table
CREATE TABLE components (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL,
    settings NVARCHAR(MAX) NOT NULL
);

-- Create 'page_components' table
CREATE TABLE page_components (
    page_id INT NOT NULL,
    component_id INT NOT NULL,
    ordinal INT NOT NULL,
    FOREIGN KEY (page_id) REFERENCES pages(id),
    FOREIGN KEY (component_id) REFERENCES components(id)
);