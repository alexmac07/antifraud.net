CREATE SCHEMA IF NOT EXISTS transact;
CREATE TABLE IF NOT EXISTS transact.transaction_events (
    event_id UUID PRIMARY KEY,
    transaction_id UUID NOT NULL,
    status INTEGER NOT NULL DEFAULT 1,
    is_processed BOOLEAN NOT NULL DEFAULT FALSE,
    messages TEXT NULL,
    created_at TIMESTAMP WITH TIME ZONE
);

CREATE TABLE IF NOT EXISTS transact.transactions (
    transaction_id UUID PRIMARY KEY,
    source_account_id UUID NOT NULL,
    target_account_id UUID NOT NULL,
    transfer_type INTEGER NOT NULL,
    value NUMERIC(18, 2) NOT NULL,
    status INTEGER NOT NULL DEFAULT 1, 
    created_at TIMESTAMP WITH TIME ZONE
);