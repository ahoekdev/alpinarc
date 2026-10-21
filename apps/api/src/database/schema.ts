import { pgTable, uuid, varchar } from 'drizzle-orm/pg-core';

export const lodges = pgTable('lodges', {
  id: uuid().primaryKey().defaultRandom(),
  name: varchar().notNull(),
});

export type Lodge = typeof lodges.$inferSelect;
export type NewLodge = typeof lodges.$inferInsert;
