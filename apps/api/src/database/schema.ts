import { pgTable, uuid, varchar } from 'drizzle-orm/pg-core';

export const lodges = pgTable('lodge', {
  id: uuid().primaryKey().defaultRandom(),
  name: varchar().notNull(),
});

export type Lodge = typeof lodges.$inferSelect;
