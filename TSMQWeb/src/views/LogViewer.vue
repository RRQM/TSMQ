<template>
  <div class="log-viewer">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>系统日志查看器</span>
          <el-button @click="exportLogs" :icon="Download">导出</el-button>
        </div>
      </template>

      <!-- 查询条件 -->
      <el-form :inline="true" class="filter-form" @submit.prevent="handleQuery">
        <el-form-item label="日期范围">
          <el-date-picker
            v-model="dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DD"
            style="width: 260px"
          />
        </el-form-item>

        <el-form-item label="日志级别">
          <el-select v-model="filterLevel" placeholder="全部" clearable style="width: 110px">
            <el-option label="DEBUG" value="DEBUG" />
            <el-option label="INFO" value="INFO" />
            <el-option label="WARN" value="WARN" />
            <el-option label="ERROR" value="ERROR" />
          </el-select>
        </el-form-item>

        <el-form-item label="模块">
          <el-input v-model="filterCategory" placeholder="模块名称" clearable style="width: 180px" />
        </el-form-item>

        <el-form-item label="关键字">
          <el-input v-model="filterKeyword" placeholder="日志内容" clearable style="width: 220px" />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" :icon="Search" :loading="loading" @click="handleQuery">查询</el-button>
        </el-form-item>
      </el-form>

      <el-table :data="logs" border stripe height="560" :row-class-name="getRowClassName" v-loading="loading">
        <el-table-column prop="Timestamp" label="时间" width="190">
          <template #default="{ row }">
            {{ formatTime(row.Timestamp) }}
          </template>
        </el-table-column>
        <el-table-column prop="Level" label="级别" width="90">
          <template #default="{ row }">
            <el-tag :type="getLevelType(row.Level)" size="small">
              {{ row.Level }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="Category" label="模块" width="220" show-overflow-tooltip />
        <el-table-column prop="Message" label="消息" min-width="300" show-overflow-tooltip />
        <el-table-column label="异常" width="80" align="center">
          <template #default="{ row }">
            <el-button v-if="row.Exception" link type="danger" @click="showDetail(row)">查看</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="footer-bar">
        <span class="stats">共 {{ total }} 条，当前显示 {{ logs.length }} 条</span>
        <el-pagination
          v-if="total > pageSize"
          :current-page="currentPage"
          :page-size="pageSize"
          :total="total"
          layout="prev, pager, next"
          style="margin-left: 20px"
          @current-change="handlePageChange"
        />
      </div>
    </el-card>

    <el-dialog v-model="detailVisible" title="异常详情" width="700px">
      <pre class="log-detail">{{ selectedLog?.Exception }}</pre>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Download, Search } from '@element-plus/icons-vue'
import { logApi, type LogEntry, type GetLogListResponse } from '@/api'
import { useUserStore } from '@/stores/user'

const logs = ref<LogEntry[]>([])
const total = ref(0)
const loading = ref(false)
const detailVisible = ref(false)
const selectedLog = ref<LogEntry | null>(null)

// 查询条件
const today = new Date().toISOString().slice(0, 10)
const dateRange = ref<[string, string]>([today, today])
const filterLevel = ref('')
const filterCategory = ref('')
const filterKeyword = ref('')

// 分页
const pageSize = 100
const currentPage = ref(1)

const userStore = useUserStore()

onMounted(() => {
  handleQuery()
})

const handleQuery = async () => {
  currentPage.value = 1
  await fetchLogs()
}

const handlePageChange = async (page: number) => {
  currentPage.value = page
  await fetchLogs()
}

const fetchLogs = async () => {
  loading.value = true
  try {
    const [startDate, endDate] = dateRange.value ?? [today, today]
    const res = (await logApi.getLogList({
      LoginedToken: userStore.token,
      Level: filterLevel.value || undefined,
      Category: filterCategory.value || undefined,
      Keyword: filterKeyword.value || undefined,
      StartDate: startDate,
      EndDate: endDate,
      Skip: (currentPage.value - 1) * pageSize,
      Take: pageSize
    })) as any as GetLogListResponse

    logs.value = res.Logs ?? []
    total.value = res.Total ?? logs.value.length
  } catch {
    ElMessage.error('获取日志失败')
  } finally {
    loading.value = false
  }
}

const formatTime = (timestamp: string) => {
  const date = new Date(timestamp)
  return date.toLocaleString('zh-CN')
}

const getLevelType = (level: string) => {
  const types: Record<string, any> = {
    DEBUG: '',
    INFO: 'success',
    WARN: 'warning',
    ERROR: 'danger'
  }
  return types[level] || ''
}

const getRowClassName = ({ row }: any) => {
  return `log-row-${(row.Level as string).toLowerCase()}`
}

const showDetail = (log: LogEntry) => {
  selectedLog.value = log
  detailVisible.value = true
}

const exportLogs = () => {
  if (logs.value.length === 0) {
    ElMessage.warning('暂无日志数据')
    return
  }
  const content = JSON.stringify(logs.value, null, 2)
  const blob = new Blob([content], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `logs_${new Date().getTime()}.json`
  a.click()
  URL.revokeObjectURL(url)
  ElMessage.success('日志导出成功')
}
</script>

<style scoped>
.log-viewer {
  width: 100%;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.filter-form {
  margin-bottom: 12px;
}

.footer-bar {
  display: flex;
  align-items: center;
  margin-top: 10px;
  padding: 8px 12px;
  background: #f5f7fa;
  border-radius: 4px;
}

.stats {
  font-size: 13px;
  color: #606266;
}

.log-detail {
  background: #1e1e1e;
  color: #d4d4d4;
  padding: 15px;
  border-radius: 4px;
  max-height: 500px;
  overflow-y: auto;
  font-size: 12px;
  line-height: 1.5;
  white-space: pre-wrap;
  word-break: break-all;
}

:deep(.log-row-debug) td {
  background-color: #f0f9ff !important;
}

:deep(.log-row-info) td {
  background-color: #f0fdf4 !important;
}

:deep(.log-row-warn) td {
  background-color: #fffbeb !important;
}

:deep(.log-row-error) td {
  background-color: #fef2f2 !important;
}
</style>
