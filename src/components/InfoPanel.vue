<template>
  <div>
    <b-sidebar
      id="sidepanel"
      title="Node information"
      right
      shadow=true
      width="450px"
      v-model="isPanelOpen">
      <div class="px-3 py-2">
        <div id="error">
          <p>The selected node does not exist. <br><br> server_id:</p>
          <p id="errorid">"{{ errorId }}"</p>
        </div>
        <div id="info">
          <json-view :data="nodeData" :root-key="rootName"/>
        </div>
      </div>
    </b-sidebar>
  </div>
</template>

<script lang="ts">
import Varz from './Graph/Varz';
import { Component, Prop, Vue } from 'vue-property-decorator'
import SidebarPanel from 'bootstrap-vue'

@Component
export default class InfoPanel extends Vue {

  isPanelOpen: boolean = false
  nodeData: Varz = { server_id: "" }
  rootName: string = ""
  errorId: string = ""

  onNodeClick (nodeData: Varz, id: string) {

    if (!this.isPanelOpen) {
      this.isPanelOpen = true
    }

    const info = document.getElementById("info")
    const error = document.getElementById("error")

    if (nodeData.server_id === "") {
      info!.style.display = "none"
      error!.style.display = "block"
      this.errorId = id
    } else {
      info!.style.display = "block"
      error!.style.display = "none"
      this.nodeData = nodeData
    }
  }

}

</script>

<style scoped>
#errorid {
  word-wrap: break-word;
  color: #268bd2;
  font-weight: 600;
  margin-top: -15px;
}

#error {
  font-weight: 600;
}
</style>
